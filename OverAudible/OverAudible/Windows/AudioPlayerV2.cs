using OverAudible.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using System.IO;
using System.Timers;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OverAudible.Windows
{
    public partial class AudioPlayerV2 : ObservableObject
    {
        private const string _audioFileExtension = ".m4b";
        private const string _timeFormat = "hh':'mm':'ss";
        private const int _timerInterval = 1000;
        private readonly IDataService<Item> _dataService;
        private string _folder;
        private string _asin;
        private List<FileInfo> _bookParts;
        private List<FileInfo> _allFiles;
        public AudibleApi.Common.ContentMetadata _contentMetadata { get; set; }
        private Timer _timer;

        public event Action? PlaybackStarted;
        public event Action? PlaybackStopped;
        public event Action<TimeSpan>? Skipped;
        public event Action<AudibleApi.Common.Chapter>? ChapterChanged;
        public event Action<ElapsedEventArgs>? TimerTicked;
        public event Action EndOfFileReached;


        [ObservableProperty]
        bool isPlaying;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentChapter))]
        int currentChapterIndex;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormatEllapsedTime))]
        int ellapsedTime;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormatRemainingTime))]
        int remainingTime;

        public PlaylistV2 Playlist { get; set; }
        public AudibleApi.Common.Chapter CurrentChapter => _contentMetadata.ChapterInfo.Chapters[CurrentChapterIndex];

        public string FormatEllapsedTime => TimeSpan.FromSeconds(EllapsedTime).ToString(_timeFormat);
        public string FormatRemainingTime => TimeSpan.FromSeconds(RemainingTime).ToString(_timeFormat);

        public bool IsAtEndOfFile { get; private set; }

        public AudioPlayerV2(IDataService<Item> dataService, string folder)
        {
            _dataService = dataService;
            _folder = folder;
            _bookParts = new List<FileInfo>();
            _allFiles = new List<FileInfo>();
            _contentMetadata = new();
            _timer = new(_timerInterval);
            Init();
        }

        private void Init()
        {
            GetFiles();
            GetMetadata();
            CreatePlayer();
            Initialize();
        }

        private void GetFiles()
        {
            string[] files = Directory.GetFiles(_folder);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                _allFiles.Add(fileInfo);
                
                if (fileInfo.Extension == _audioFileExtension)
                    _bookParts.Add(fileInfo);
            }
        }

        private void GetMetadata()
        {
            string filter = "_Cover";
            char startFilterChar = filter[0];
            _asin = _allFiles.FirstOrDefault(x => x.Name.Contains(filter), null).Name;

            if (!string.IsNullOrWhiteSpace(_asin))
            {
                _asin = _asin.Remove(_asin.IndexOf(startFilterChar));
                _contentMetadata = _dataService.GetByIdWithMetadata(_asin).GetAwaiter().GetResult().Item2;
            }
        }

        private void CreatePlayer()
        {
            Playlist = new(_bookParts.Select(x => x.FullName).ToList());
        }

        private void Initialize()
        {
            _timer.Elapsed += OnTimerEllapsed;
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
        }

        private void OnTimerEllapsed(object? sender, ElapsedEventArgs e)
        {
            if (CurrentChapterIndex == _contentMetadata.ChapterInfo.Chapters.Length - 1 && EllapsedTime + 1 == GetSeconds(CurrentChapter.LengthMs))
            {
                EndOfFile();
                return;
            }
            EllapsedTime++;
            RemainingTime--;

            if (EllapsedTime == GetSeconds(CurrentChapter.LengthMs))
                ChangeChapter(1);
            else
                TimerTicked?.Invoke(e);
        }

        private int GetSeconds(long lengthMs)
        {
            return (int)(lengthMs / 1000);
        }

        private int GetSeconds(double lengthMs)
        {
            return (int)(lengthMs / 1000);
        }

        public void Play()
        {
            IsPlaying = true;
            Playlist.Play();
            _timer.Start();
            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            IsPlaying = false;
            Playlist.Pause();
            _timer.Stop();
            PlaybackStopped?.Invoke();
        }

        private void EndOfFile()
        {
            IsAtEndOfFile = true;
            EndOfFileReached?.Invoke();
            Pause();
            ChangeChapter(_contentMetadata.ChapterInfo.Chapters[0]);
            IsAtEndOfFile = false;
        }

        public void Skip(TimeSpan amount, Direction direction)
        {
            var c = Playlist.GetTotalTimeUpToCurrentReader().Add(Playlist.GetCurrentPosition());
            var add = (int)(c.Add(amount).TotalMilliseconds / 1000);
            var sub = (int)(Playlist.GetCurrentPosition().Subtract(amount).TotalMilliseconds / 1000);
            var total = (int)(TotalUpToCurrentChapterMillieseconds() / 1000);

            if (add >= total && direction == Direction.Forward)
            {
                ChangeChapter(1);
                return;
            }

            if (sub <= CurrentChapter.StartOffsetMs && direction == Direction.Backward)
            {
                ChangeChapter(-1);
                return;
            }

            switch (direction)
            {
                case Direction.Forward:
                    if (Playlist.GetCurrentPosition().Add(amount) > Playlist.GetTotalTime())
                        return;
                    Playlist.Skip(amount,false);
                    EllapsedTime += (int)amount.TotalSeconds;
                    RemainingTime -= (int)amount.TotalSeconds;
                    Skipped?.Invoke(amount);
                    break;
                case Direction.Backward:
                    if (Playlist.GetCurrentPosition().Subtract(amount) < TimeSpan.Zero && CurrentChapter == _contentMetadata.ChapterInfo.Chapters[0])
                        return;
                    Playlist.Skip(amount, true);
                    EllapsedTime -= (int)amount.TotalSeconds;
                    RemainingTime += (int)amount.TotalSeconds;
                    Skipped?.Invoke(amount);
                    break;
            }
           
        }

        private double TotalUpToCurrentChapterMillieseconds()
        {
            long ms = 0;
            for (int i = 0; i <= CurrentChapterIndex; i++)
            {
                ms += _contentMetadata.ChapterInfo.Chapters[i].LengthMs;
            }

            return ms;
        }

        public void ChangeChapter(AudibleApi.Common.Chapter chapter)
        {
            int index = _contentMetadata.ChapterInfo.Chapters.ToList().IndexOf(chapter);

            if (index < 0 || index > _contentMetadata.ChapterInfo.Chapters.Length)
            {

                EndOfFile();
                return;
            }



            CurrentChapterIndex = index;
            EllapsedTime = 0;

            if (CurrentChapter == _contentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - GetSeconds(Playlist.GetTotalTime().TotalMilliseconds);
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }

            Playlist.GoToPosition(TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec));
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        public void ChangeChapter(int direction)
        {
            if (CurrentChapterIndex + direction < 0 || CurrentChapterIndex + direction >= _contentMetadata.ChapterInfo.Chapters.Length)
            {
                EndOfFile();
                return;
            }

            CurrentChapterIndex += direction;
            EllapsedTime = 0;

            if (CurrentChapter == _contentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - GetSeconds(Playlist.GetTotalTime().TotalMilliseconds);
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }

            Playlist.GoToPosition(TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec));
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        private long TotalChapterSeconds()
        {
            return _contentMetadata.ChapterInfo.Chapters.Sum(x => x.LengthMs) / 1000;
        }

        public enum Direction
        {
            Forward,
            Backward,
        }

    }
}
