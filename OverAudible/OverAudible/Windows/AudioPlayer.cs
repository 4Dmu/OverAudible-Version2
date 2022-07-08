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
}
