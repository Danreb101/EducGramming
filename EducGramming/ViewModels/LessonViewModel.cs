using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducGramming.Views;

namespace EducGramming.ViewModels
{
    public class LessonItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
    }

    public partial class LessonViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedLanguage = "C#";

        [ObservableProperty]
        private string _currentTab = "Lessons";

        [ObservableProperty]
        private ObservableCollection<LessonItem> _lessons = new();

        [ObservableProperty]
        private ObservableCollection<LessonItem> _videos = new();

        [ObservableProperty]
        private ObservableCollection<LessonItem> _quizzes = new();

        public LessonViewModel()
        {
            LoadContent();
        }

        [RelayCommand]
        private void ChangeTab(string tabName)
        {
            CurrentTab = tabName;
        }

        [RelayCommand]
        private async Task OpenVideo(LessonItem video)
        {
            if (video == null || string.IsNullOrEmpty(video.VideoUrl)) return;

            try
            {
                var videoPlayer = new VideoPlayerPage(video.Title, video.Description, video.VideoUrl);
                await Application.Current.MainPage.Navigation.PushModalAsync(videoPlayer);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Could not play video: " + ex.Message, "OK");
            }
        }

        private void LoadContent()
        {
            // Clear existing collections
            Lessons.Clear();
            Videos.Clear();
            Quizzes.Clear();

            if (SelectedLanguage == "C#")
            {
                LoadCSharpContent();
            }
            else
            {
                LoadJavaContent();
            }
        }

        private void LoadCSharpContent()
        {
            // Add C# lessons
            Lessons.Add(new LessonItem 
            { 
                Title = "Introduction to C#",
                Description = "Learn the basics of C# programming including variables, data types, and control structures.",
                Language = "C#",
                Type = "Lesson"
            });
            Lessons.Add(new LessonItem 
            { 
                Title = "Object-Oriented Programming in C#",
                Description = "Understanding classes, objects, inheritance, and polymorphism in C#.",
                Language = "C#",
                Type = "Lesson"
            });

            // Add C# videos
            Videos.Add(new LessonItem
            {
                Title = "C# Variables and Types",
                Description = "A comprehensive guide to variables and data types in C#",
                Language = "C#",
                Type = "Video",
                VideoUrl = Path.Combine(FileSystem.AppDataDirectory, "Videos", "Csharp", "variables.mp4")
            });
            Videos.Add(new LessonItem
            {
                Title = "C# Control Structures",
                Description = "Learn about if statements, loops, and switch cases in C#",
                Language = "C#",
                Type = "Video",
                VideoUrl = Path.Combine(FileSystem.AppDataDirectory, "Videos", "Csharp", "control-structures.mp4")
            });

            // Add C# quizzes
            Quizzes.Add(new LessonItem
            {
                Title = "C# Basics Quiz",
                Description = "Test your knowledge of C# fundamentals",
                Language = "C#",
                Type = "Quiz"
            });
            Quizzes.Add(new LessonItem
            {
                Title = "C# OOP Quiz",
                Description = "Test your understanding of Object-Oriented Programming in C#",
                Language = "C#",
                Type = "Quiz"
            });
        }

        private void LoadJavaContent()
        {
            // Add Java lessons
            Lessons.Add(new LessonItem 
            { 
                Title = "Introduction to Java",
                Description = "Learn the basics of Java programming including variables, data types, and control structures.",
                Language = "Java",
                Type = "Lesson"
            });
            Lessons.Add(new LessonItem 
            { 
                Title = "Object-Oriented Programming in Java",
                Description = "Understanding classes, objects, inheritance, and polymorphism in Java.",
                Language = "Java",
                Type = "Lesson"
            });

            // Add Java videos
            Videos.Add(new LessonItem
            {
                Title = "Java Variables and Types",
                Description = "A comprehensive guide to variables and data types in Java",
                Language = "Java",
                Type = "Video",
                VideoUrl = Path.Combine(FileSystem.AppDataDirectory, "Videos", "Java", "variables.mp4")
            });
            Videos.Add(new LessonItem
            {
                Title = "Java Control Structures",
                Description = "Learn about if statements, loops, and switch cases in Java",
                Language = "Java",
                Type = "Video",
                VideoUrl = Path.Combine(FileSystem.AppDataDirectory, "Videos", "Java", "control-structures.mp4")
            });

            // Add Java quizzes
            Quizzes.Add(new LessonItem
            {
                Title = "Java Basics Quiz",
                Description = "Test your knowledge of Java fundamentals",
                Language = "Java",
                Type = "Quiz"
            });
            Quizzes.Add(new LessonItem
            {
                Title = "Java OOP Quiz",
                Description = "Test your understanding of Object-Oriented Programming in Java",
                Language = "Java",
                Type = "Quiz"
            });
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            LoadContent();
        }
    }
} 