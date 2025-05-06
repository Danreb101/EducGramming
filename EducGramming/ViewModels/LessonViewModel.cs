using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EducGramming.Models;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class LessonViewModel : INotifyPropertyChanged
    {
        private bool _isVideoPlaying;
        private double _videoProgress;
        private Lesson _selectedLesson;
        private ObservableCollection<Lesson> _lessons;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set
            {
                if (_isVideoPlaying != value)
                {
                    _isVideoPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VideoProgress
        {
            get => _videoProgress;
            set
            {
                if (_videoProgress != value)
                {
                    _videoProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public Lesson SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                if (_selectedLesson != value)
                {
                    _selectedLesson = value;
                    OnPropertyChanged();
                    LoadVideo();
                }
            }
        }

        public ObservableCollection<Lesson> Lessons
        {
            get => _lessons;
            set
            {
                if (_lessons != value)
                {
                    _lessons = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand PlayVideoCommand { get; }

        public LessonViewModel()
        {
            PlayVideoCommand = new Command(PlayVideo);
            InitializeLessons();
        }

        private void InitializeLessons()
        {
            Lessons = new ObservableCollection<Lesson>
            {
                new Lesson { Number = 1, Title = "Introduction to Programming", Description = "Learn the basics of programming concepts", VideoUrl = "video1.mp4", IsCompleted = true },
                new Lesson { Number = 2, Title = "Variables and Data Types", Description = "Understanding different types of data", VideoUrl = "video2.mp4", IsCompleted = true },
                new Lesson { Number = 3, Title = "Control Structures", Description = "If statements and loops", VideoUrl = "video3.mp4", IsCompleted = false },
                new Lesson { Number = 4, Title = "Functions and Methods", Description = "Creating reusable code blocks", VideoUrl = "video4.mp4", IsCompleted = false },
                new Lesson { Number = 5, Title = "Object-Oriented Programming", Description = "Classes and objects", VideoUrl = "video5.mp4", IsCompleted = false },
                new Lesson { Number = 6, Title = "Advanced Topics", Description = "Advanced programming concepts", VideoUrl = "video6.mp4", IsCompleted = false }
            };
        }

        private void PlayVideo()
        {
            if (SelectedLesson == null) return;

            IsVideoPlaying = !IsVideoPlaying;
            // TODO: Implement actual video playback
        }

        private void LoadVideo()
        {
            if (SelectedLesson == null) return;

            VideoProgress = 0;
            IsVideoPlaying = false;
            // TODO: Load the video for the selected lesson
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }
} 