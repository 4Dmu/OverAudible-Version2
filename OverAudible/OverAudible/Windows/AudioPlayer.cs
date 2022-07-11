using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using OverAudible.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using OverAudible.Models;
using NAudio.Wave.SampleProviders;
using ShellUI.Controls;

namespace OverAudible.Windows
{
    public partial class AudioPlayer : ObservableObject
    {
        private readonly IDataService<Item> _dataService;
        private AudioFileReader Reader;
        private WaveOutEvent OutputDevice;
        private ConcatenatingSampleProvider Playlist;
        private string _asin;
        private Timer Timer;

        public Playlist p;

        public bool IsAtEndOfFile { get; set; }

        private List<FileInfo> BookParts { get; set; }
        private List<FileInfo> Files { get; set; }
        public AudibleApi.Common.ContentMetadata ContentMetadata { get; set; }

        [ObservableProperty]
        bool isPlaying;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentChapter))]
        private int currentChapterNum;

        public event Action? PlaybackStarted;
        public event Action? PlaybackStoped;
        public event Action? SkippedForward;
        public event Action? SkippedBack;
        public event Action<AudibleApi.Common.Chapter>? ChapterChanged;
        public event Action PlayerStopped;
        public event Action EndOfFileReached;
        public event Action<ElapsedEventArgs>? TimerTick;

        public AudibleApi.Common.Chapter CurrentChapter => ContentMetadata.ChapterInfo.Chapters[CurrentChapterNum];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormatEllapsedTime))]
        int ellapsedTime;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormatRemainingTime))]
        int remainingTime;

        private const string _timeFormat = "mm':'ss";

        public string FormatEllapsedTime => TimeSpan.FromSeconds(EllapsedTime).ToString(_timeFormat);
        public string FormatRemainingTime => TimeSpan.FromSeconds(RemainingTime).ToString(_timeFormat);

        public AudioPlayer(IDataService<Item> dataService,string folder)
        {
            _dataService = dataService;
            Timer = new Timer(1000);
            Timer.Elapsed += OnTimerTick;
            BookParts = new();
            Files = new();
            ContentMetadata = new AudibleApi.Common.ContentMetadata();
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Init(folder);
            }

        }

        private void Init(string folder)
        {
            GetBookParts(folder);
            GetMetadata(folder);
            CreateOutputDevice();
            CreateAudioFile();
            RemainingTime = (int)(CurrentChapter.LengthMs / 1000);
            p = new(BookParts.Select(x => x.FullName).ToList());
            p.Play();

        }

        private void GetMetadata(string folder)
        {
            string[] files = Directory.GetFiles(folder);

            string? asin = files.FirstOrDefault(x => x.Contains("_Cover"), null);

            if (asin != null)
            {
                asin = new FileInfo(asin).Name;
                asin = asin.Remove(asin.IndexOf('_'));
                _asin = asin;
                var metadata = _dataService.GetByIdWithMetadata(asin).GetAwaiter().GetResult();
                ContentMetadata = metadata.Item2;

                
            }
        }

        private void CreateAudioFile()
        {
            FileInfo? merged = Files.FirstOrDefault(x => x.Name.Contains("_Merged.wav"), null);

            if (merged != null)
            {
                Reader = new AudioFileReader(merged.FullName);
            }
            else
            {
                Shell.Current.CurrentPage.DisplayAlert("Alert", "Currently merging audio files, please wait.");
                List<AudioFileReader> readers = new();
                foreach (var file in BookParts)
                {
                    readers.Add(new AudioFileReader(file.FullName));
                }
                Playlist = new ConcatenatingSampleProvider(readers);

                

                WaveFileWriter.CreateWaveFile16(BookParts[0].Directory + @$"\{_asin}_Merged.wav", Playlist);
            }

            OutputDevice.Init(Reader);
            
        }

        private void CreateOutputDevice()
        {
            OutputDevice = new WaveOutEvent();
            OutputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            PlayerStopped?.Invoke();
        }

        private void GetBookParts(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                Files.Add(info);
                if (info.Extension == ".m4b")
                    BookParts.Add(info);
            }
        }

        private void OnTimerTick(object? sender, ElapsedEventArgs e)
        {
            int currentChapterLength = (int)(CurrentChapter.LengthMs / 1000);
            bool val1 = CurrentChapterNum == ContentMetadata.ChapterInfo.Chapters.Length - 1;
            bool val2 = EllapsedTime + 1 == currentChapterLength;
            if ( val1 && val2)
            {
                EndOfFile();
                return;
            }

            EllapsedTime += 1;
            RemainingTime -= 1;
            
            if (EllapsedTime == currentChapterLength)
            {
                CurrentChapterNum += 1;
                RemainingTime = (int)(CurrentChapter.LengthMs / 1000);
                EllapsedTime = 0;
                ChapterChanged?.Invoke(CurrentChapter);
            }
            else
            {
                TimerTick?.Invoke(e);
            }

        }

        public void ChangeChapter(AudibleApi.Common.Chapter chapter)
        {
            int index = ContentMetadata.ChapterInfo.Chapters.ToList().IndexOf(chapter);
            //index += 1;

            if (index < 0 || index > ContentMetadata.ChapterInfo.Chapters.Length)
                return;

            CurrentChapterNum = index;
            EllapsedTime = 0;

            Reader.CurrentTime = TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec);

            if (CurrentChapter == ContentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - (int)Reader.TotalTime.TotalSeconds;
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }

            RemainingTime = (int)(CurrentChapter.LengthMs / 1000);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        ///<summary>
        ///<para>Either pass a chapter or a 1 or -1 to go</para>
        ///<para>to the next or previous chapterso</para>
        ///</summary>
        public void ChangeChapter(int direction)
        {
            if (CurrentChapterNum + direction < 0 || CurrentChapterNum + direction >= ContentMetadata.ChapterInfo.Chapters.Length)
                return;

            CurrentChapterNum += direction;
            EllapsedTime = 0;
            Reader.CurrentTime = TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec);

            if (CurrentChapter == ContentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - (int)Reader.TotalTime.TotalSeconds;
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }


            RemainingTime = (int)(CurrentChapter.LengthMs / 1000);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        public void Play()
        {
            IsPlaying = true;
            OutputDevice.Play();
            Timer.Start();
            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            IsPlaying = false;
            OutputDevice.Pause();
            Timer.Stop();
            PlaybackStoped?.Invoke();
        }

        public void EndOfFile()
        {
            IsAtEndOfFile = true;
            EndOfFileReached?.Invoke();
            Pause();
        }

        public void SkipBack(TimeSpan amount)
        {
            if (Reader.CurrentTime.Subtract(amount).TotalSeconds < 0)
                return;
            Reader.CurrentTime = Reader.CurrentTime.Subtract(amount);

            if (Reader.CurrentTime.TotalMilliseconds < CurrentChapter.StartOffsetMs)
            {
                ChangeChapter(-1);
                SkippedBack?.Invoke();
                return;
            }
            EllapsedTime -= (int)amount.TotalSeconds;
            RemainingTime += (int)amount.TotalSeconds;
            SkippedBack?.Invoke();
        }

        public void SkipForward(TimeSpan amount)
        {
            var m = Reader.CurrentTime.Add(amount);
            var ts = TotalChapterSeconds();
            if (m.TotalSeconds > ts)
                return;

            Reader.CurrentTime = Reader.CurrentTime.Add(amount);
            var ms = TotalUpToCurrentChapterMs();
            if (Reader.CurrentTime.TotalMilliseconds > ms)
            {
                ChangeChapter(1);
                SkippedForward?.Invoke();
                return;
            }
            EllapsedTime += (int)amount.TotalSeconds;
            RemainingTime -= (int)amount.TotalSeconds;
            SkippedForward?.Invoke();
        }

        private long TotalUpToCurrentChapterMs()
        {
            long ms = 0;
            for (int i = 0; i <= CurrentChapterNum; i++)
            {
                ms += ContentMetadata.ChapterInfo.Chapters[i].LengthMs;
            }

            return ms;
        }

        private long TotalChapterSeconds()
        {
            return ContentMetadata.ChapterInfo.Chapters.Sum(x => x.LengthMs) / 1000;
        }
    }


    public class AudioPlaylist
    {
        private Queue<string> playlist;
        private IWavePlayer player;
        private WaveStream fileWaveStream;

        public AudioPlaylist(List<string> startingPlaylist)
        {
            playlist = new Queue<string>(startingPlaylist);
        }

        public void PlaySong()
        {
            if (playlist.Count < 1)
            {
                return;
            }

            if (player != null && player.PlaybackState != PlaybackState.Stopped)
            {
                player.Stop();
            }
            if (fileWaveStream != null)
            {
                fileWaveStream.Dispose();
            }
            if (player != null)
            {
                player.Dispose();
                player = null;
            }

            player = new WaveOutEvent();
            fileWaveStream = new AudioFileReader(playlist.Dequeue());
            player.Init(fileWaveStream);
            player.PlaybackStopped += (sender, evn) => { PlaySong(); };
            player.Play();
        }
    }

    public class PlaylistV2 : IDisposable
    {
        private List<AudioFileReader> _readers;
        private WaveOutEvent? _outputDevice;
        private List<string> _files;
        private int currentReader = 0;
        private bool disposed = false;
        private bool canBeStopped;

        public PlaylistV2(List<string> files)
        {
            _files = files;
            _readers = new List<AudioFileReader>();
            _outputDevice = new();
            Init();
        }

        private void Init()
        {
            foreach (var f in _files)
            {
                _readers.Add(new AudioFileReader(f));
            }
            _outputDevice.Init(_readers[0]);
            _outputDevice.PlaybackStopped += (s, e) =>
            {
                if (canBeStopped)
                {
                    canBeStopped = false;
                    return;
                }
                ChangeReader();
                _outputDevice.Play();
            };
        }

        private void ChangeReader()
        {
            currentReader++;
            if (currentReader > _readers.Count)
            {
                currentReader = 0;
            }
            _outputDevice.Init(_readers[currentReader]);
        }

        private void ChangeReader(int num)
        {
            currentReader = num;
            if (currentReader > _readers.Count)
            {
                currentReader = 0;
            }
            bool isPlaying = _outputDevice.PlaybackState == PlaybackState.Playing;
            canBeStopped = true;
            _outputDevice.Stop();
            _outputDevice.Init(_readers[currentReader]);
            if (isPlaying)
                _outputDevice.Play();
        }

        public TimeSpan GetCurrentPosition()
        {
            return _readers[currentReader].CurrentTime;
        }

        public TimeSpan GetCurrentReaderTotalTime()
        {
            return _readers[currentReader].TotalTime;
        }

        public TimeSpan GetTotalTime()
        {
            return _readers
                .Select(x => x.TotalTime)
                .Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
        }

        public TimeSpan GetTotalTimeUpToCurrentReader()
        {
            return _readers
                .GetRange(0,currentReader)
                .Select(x => x.TotalTime)
                .Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
        }

        public void Skip(TimeSpan amount, bool goBack = false)
        {
            if (goBack)
            {
                var pos = GetCurrentPosition().Subtract(amount);
                if (pos < TimeSpan.Zero)
                {
                    if (currentReader == 0)
                        return;
                    ChangeReader();
                    Skip(amount, goBack);
                }

                _readers[currentReader].CurrentTime = _readers[currentReader].CurrentTime.Subtract(amount);
            }
            else
            {
                var pos = GetCurrentPosition().Add(amount);
                var time = GetCurrentReaderTotalTime();
                if (pos > time)
                {
                    ChangeReader();
                    Skip(amount, goBack);
                }

                _readers[currentReader].CurrentTime = _readers[currentReader].CurrentTime.Add(amount);
            }
        }

        public void GoToPosition(TimeSpan position)
        {
            List<TimeSpan> readerLengths = _readers.Select(x => x.TotalTime).ToList();
            TimeSpan totalReaderTime = GetTotalTime();
            
            foreach (TimeSpan readerLength in readerLengths)
            {
                int index = readerLengths.IndexOf(readerLength);
                
                var length = readerLengths.GetRange(0, index + 1).Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
                if (position <= length)
                {
                    ChangeReader(readerLengths.IndexOf(readerLength));

                    if (readerLength == readerLengths[0])
                    {
                        _readers[currentReader].CurrentTime = position;
                    }
                    else
                    {
                        var totalTime = GetTotalTimeUpToCurrentReader();
                        var newPos = position.Subtract(totalTime);
                        _readers[currentReader].CurrentTime = newPos;
                    }

                    return;
                }
            }
        }

        public void Play()
        {
            _outputDevice?.Play();
        }

        public void Pause()
        {
            _outputDevice?.Pause();
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _outputDevice.Dispose();
                    foreach (var reader in _readers)
                    {
                        reader.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }

    public class Playlist
    {
        private AudioFileReader? Reader;
        private WaveOutEvent? OutputDevice;
        private List<string> TotalFiles { get; set; }

        private List<TimeSpan> FileLengths { get; set; }
        private string currentFile;
        private Queue<string> Files { get; set; }

        public Playlist(List<string> files)
        {
            Files = new(files);
            TotalFiles = files;
            FileLengths = new();
            Init();
        }

        private void Init()
        {
            OutputDevice = new();
            var f = Files.Dequeue();
            currentFile = f;
            Reader = new(f);
            OutputDevice.Init(Reader);
            OutputDevice.PlaybackStopped += (s, e) =>
            {
                if (Files.Count == 0)
                {
                    Files = new Queue<string>(TotalFiles);
                }

                Init();
                Play();
            };
        }

        public void Play()
        {
            OutputDevice?.Play();
        }

        public void Pause()
        {
            OutputDevice?.Pause();
        }

        public void Skip(TimeSpan amount, AudioPlayerV2.Direction direction)
        {
            if (Reader.CurrentTime.Add(amount) > Reader.TotalTime)
            {
                if (Files.Count == TotalFiles.Count)
                    return;
                Init();
                Skip(amount, direction);
                return;
            }

            switch (direction)
            {
                case AudioPlayerV2.Direction.Forward:
                    Reader.CurrentTime = Reader.CurrentTime.Add(amount);
                    break;
                case AudioPlayerV2.Direction.Backward:
                    if (Reader.CurrentTime.Subtract(amount) < TimeSpan.Zero)
                        return;
                    Reader.CurrentTime = Reader.CurrentTime.Subtract(amount);
                    break;
            }
        }

        public void GoToPosition(TimeSpan positionToGoTo)
        {
            if (positionToGoTo < TimeSpan.Zero)
                return;
            var currentFileTotalTime = Reader.TotalTime;
            var timeUpToCurrentFile = GetTimeUpToCurrentFile();

            var finalTime = positionToGoTo;

            if (currentFile != TotalFiles[0])
                finalTime = positionToGoTo.Subtract(timeUpToCurrentFile);

            


            if (finalTime > currentFileTotalTime)
            {
                if (Files.Count == TotalFiles.Count)
                    return;
                Init();
                GoToPosition(positionToGoTo);
                return;
            }

            
            
            Reader.CurrentTime = finalTime;
        }

        public TimeSpan GetCurrentPosition()
        {
            return Reader.CurrentTime;
        }

        public TimeSpan GetCurrentFileTotalTime()
        {
            return Reader.TotalTime;
        }

        public TimeSpan GetTotalTime()
        {
            TimeSpan t = new();
            FileLengths.Clear();
            foreach (var file in TotalFiles)
            {
                var reader = new AudioFileReader(file);
                t = t.Add(reader.TotalTime);
                FileLengths.Add(reader.TotalTime);
                reader.Dispose();
            }
            return t;
        }

        private TimeSpan GetTimeUpToCurrentFile()
        {
            int index = TotalFiles.IndexOf(currentFile);
            TimeSpan t = new();
            for (int i = 0; i < index; i++)
            {
                var reader = new AudioFileReader(TotalFiles[i]);
                t = t.Add(reader.TotalTime);
            }
            return t;
        }
    }


}
