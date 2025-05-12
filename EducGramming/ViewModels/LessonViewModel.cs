using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducGramming.Views;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class LessonSection : INotifyPropertyChanged
    {
        private string _title;
        private bool _isExpanded;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title 
        { 
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        public ObservableCollection<LessonItem> Lessons { get; set; } = new();
    }

    public class LessonItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _title = string.Empty;
        private bool _isCompleted;
        private bool _isLocked;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public int Number { get; set; }
        
        public string Title 
        { 
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        
        public bool IsSelected 
        { 
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusIcon));
                }
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusIcon));
                }
            }
        }

        public string StatusIcon
        {
            get
            {
                if (IsLocked) return "🔒";
                if (IsCompleted) return "✅";
                return "🔓";
            }
        }
        
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public partial class LessonViewModel : ObservableObject
    {
        private int _currentLessonIndex;
        private bool _isInitialized;
        private int _lives = 3;
        private DateTime _lastActionTime = DateTime.MinValue;
        private const int ACTION_COOLDOWN_SECONDS = 30;
        
        public int Lives
        {
            get => _lives;
            set
            {
                if (_lives != value)
                {
                    _lives = value;
                    OnPropertyChanged(nameof(Lives));
                }
            }
        }

        // Check if enough time has passed since the last action
        private bool CanPerformAction()
        {
            var timeSinceLastAction = DateTime.Now - _lastActionTime;
            return timeSinceLastAction.TotalSeconds >= ACTION_COOLDOWN_SECONDS;
        }

        // Update the last action timestamp
        private void UpdateActionTimestamp()
        {
            _lastActionTime = DateTime.Now;
        }

        public void ResetLives()
        {
            // Always reset to exactly 3 lives
            Lives = 3;
            
            // Ensure UI updates
            OnPropertyChanged(nameof(Lives));
        }

        // Method to reset the current lesson
        public async Task RestartCurrentLessonAsync()
        {
            // Check if the action is allowed based on cooldown
            if (!CanPerformAction())
            {
                // If action was performed too recently, ignore
                return;
            }
            
            // Update the timestamp
            UpdateActionTimestamp();
            
            // Reset hearts to 3
            ResetLives();

            // Reload the current lesson
            if (SelectedLesson != null)
            {
                var section = LessonSections.FirstOrDefault(s => s.Lessons.Contains(SelectedLesson));
                if (section != null)
                {
                    int index = section.Lessons.IndexOf(SelectedLesson);
                    await SelectLessonAsync(index);
                }
            }
        }

        public int CurrentLessonIndex
        {
            get => _currentLessonIndex;
            set
            {
                if (_currentLessonIndex != value)
                {
                    _currentLessonIndex = value;
                    if (_isInitialized)
                    {
                        LoadLessonByIndex(value);
                    }
                }
            }
        }

        [ObservableProperty]
        private string _selectedLanguage = "C#";

        [ObservableProperty]
        private LessonItem _selectedLesson;

        [ObservableProperty]
        private LessonItem _currentVideo;

        [ObservableProperty]
        private ObservableCollection<LessonSection> _lessonSections = new();

        [ObservableProperty]
        private ObservableCollection<LessonItem> _currentQuestions = new();

        [ObservableProperty]
        private bool _isSidebarVisible = true;

        [ObservableProperty]
        private double _sidebarWidth = 300;

        [ObservableProperty]
        private string _lessonContent;

        [ObservableProperty]
        private bool _isPlaying;
        
        [ObservableProperty]
        private bool _isLoading;

        public LessonViewModel()
        {
            LoadContent();
            _isInitialized = true;
        }

        public async Task HandleMenuItemSelection(string menuId)
        {
            if (string.IsNullOrEmpty(menuId)) return;
            
            try
            {
                IsLoading = true;
                
                // Parse the menu ID to get section and item indices
                var parts = menuId.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lessonIndex))
                {
                    await SelectLessonAsync(lessonIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling menu selection: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            try
            {
                IsSidebarVisible = !IsSidebarVisible;
                
                if (IsSidebarVisible && SelectedLesson != null)
                {
                    // When opening sidebar, ensure correct section is expanded
                    foreach (var section in LessonSections)
                    {
                        section.IsExpanded = section.Lessons.Any(l => l.IsSelected);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling sidebar: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleSection(LessonSection section)
        {
            if (section == null) return;
            
            try
            {
                // Close other sections when opening a new one
                foreach (var s in LessonSections)
                {
                    if (s != section)
                    {
                        s.IsExpanded = false;
                    }
                }
                section.IsExpanded = !section.IsExpanded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling section: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SelectLessonAsync(int index)
        {
            try
            {
                var section = LessonSections.FirstOrDefault(s => 
                    s.Title.StartsWith(SelectedLanguage, StringComparison.OrdinalIgnoreCase));
                    
                if (section == null || index < 0 || index >= section.Lessons.Count)
                {
                    Debug.WriteLine($"Invalid lesson index: {index}");
                    return;
                }

                var lesson = section.Lessons[index];
                
                // Always unlock the first lesson
                if (index == 0 && lesson.IsLocked)
                {
                    lesson.IsLocked = false;
                    lesson.OnPropertyChanged("StatusIcon");
                }
                
                if (lesson == null || (lesson.IsLocked && index > 0))
                {
                    // Show message if lesson is locked but not the first lesson
                    if (lesson?.IsLocked == true && index > 0)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Lesson Locked",
                            "Please complete the previous lesson to unlock this one.",
                            "OK"
                        );
                    }
                    return;
                }

                // Always reset lives to 3 when selecting a lesson
                ResetLives();
                
                // Reset action timestamp when selecting a new lesson
                UpdateActionTimestamp();

                // Update selection state
                foreach (var s in LessonSections)
                {
                    foreach (var item in s.Lessons)
                    {
                        item.IsSelected = (item == lesson);
                    }
                }

                SelectedLesson = lesson;
                _currentLessonIndex = index;
                
                await Task.WhenAll(
                    Task.Run(() => LessonContent = GetLessonContent(lesson)),
                    Task.Run(() => LoadVideoContent(lesson)),
                    Task.Run(() => LoadQuestions(lesson))
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error selecting lesson: {ex.Message}");
            }
        }

        private void LoadLessonByIndex(int index)
        {
            try
            {
                var section = LessonSections.FirstOrDefault(s => 
                    s.Title.StartsWith(SelectedLanguage, StringComparison.OrdinalIgnoreCase));
                    
                if (section != null && index >= 0 && index < section.Lessons.Count)
                {
                    _ = SelectLessonAsync(index);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading lesson by index: {ex.Message}");
            }
        }

        private void LoadContent()
        {
            try
            {
                LessonSections.Clear();
                IsLoading = true;

                // Reset lives when loading new content
                ResetLives();

                var csharpSection = new LessonSection 
                { 
                    Title = "C# Lessons",
                    IsExpanded = SelectedLanguage == "C#"
                };
                LoadCSharpContent(csharpSection.Lessons);
                LessonSections.Add(csharpSection);

                var javaSection = new LessonSection 
                { 
                    Title = "Java Lessons",
                    IsExpanded = SelectedLanguage == "Java"
                };
                LoadJavaContent(javaSection.Lessons);
                LessonSections.Add(javaSection);

                // Select appropriate section and lesson
                var currentSection = SelectedLanguage == "C#" ? csharpSection : javaSection;
                if (currentSection.Lessons.Count > 0)
                {
                    // Load completion status
                    LoadLessonCompletionStatus(currentSection.Lessons);
                    
                    var firstLesson = currentSection.Lessons[CurrentLessonIndex];
                    firstLesson.IsSelected = true;
                    SelectedLesson = firstLesson;
                    LoadVideoContent(firstLesson);
                    LoadQuestions(firstLesson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading content: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadCSharpContent(ObservableCollection<LessonItem> lessons)
        {
            var csharpLessons = new List<(int number, string title, string description, List<string> options, string correctAnswer)>
            {
                (1, "Introduction to C#", 
                "", 
                new List<string> {
                    "C# is a programming language developed by Microsoft",
                    "C# is a database management system",
                    "C# is a web browser",
                    "C# is an operating system"
                },
                "C# is a programming language developed by Microsoft"),

                (2, "Variables and Data Types", 
                "", 
                new List<string> {
                    "int, float, string, bool are basic data types in C#",
                    "All numbers in C# are stored as text",
                    "C# only supports whole numbers",
                    "Variables cannot change their type after declaration"
                },
                "int, float, string, bool are basic data types in C#"),

                (3, "Control Structures", 
                "", 
                new List<string> {
                    "Control structures help manage program flow",
                    "Control structures are only used for mathematical operations",
                    "Loops can only run 10 times maximum",
                    "If statements can only check equality"
                },
                "Control structures help manage program flow"),

                (4, "Methods and Functions", 
                "", 
                new List<string> {
                    "Methods are reusable blocks of code that perform specific tasks",
                    "Methods can only return void",
                    "Methods cannot take parameters",
                    "Every method must be static"
                },
                "Methods are reusable blocks of code that perform specific tasks"),

                (5, "Object-Oriented Programming", 
                "", 
                new List<string> {
                    "OOP helps organize code using classes and objects",
                    "OOP is only used for game development",
                    "Classes cannot inherit from other classes",
                    "Objects cannot have properties"
                },
                "OOP helps organize code using classes and objects"),

                (6, "Collections and Arrays", 
                "", 
                new List<string> {
                    "Collections are used to store multiple values",
                    "Arrays can only store numbers",
                    "Lists cannot be modified after creation",
                    "Dictionaries can only use strings as keys"
                },
                "Collections are used to store multiple values"),

                (7, "Exception Handling", 
                "", 
                new List<string> {
                    "Exception handling helps manage runtime errors",
                    "Exceptions automatically fix all errors",
                    "Try-catch blocks slow down the program",
                    "Only system exceptions can be caught"
                },
                "Exception handling helps manage runtime errors"),

                (8, "File Operations", 
                "", 
                new List<string> {
                    "File operations allow reading and writing files",
                    "Files can only store text",
                    "Files cannot be created programmatically",
                    "File operations are always synchronous"
                },
                "File operations allow reading and writing files")
            };

            foreach (var lesson in csharpLessons)
            {
                lessons.Add(new LessonItem
                {
                    Number = lesson.number,
                    Title = lesson.title,
                    Description = lesson.description,
                    Language = "C#",
                    Type = "Lesson",
                    Options = lesson.options,
                    CorrectAnswer = lesson.correctAnswer
                });
            }
        }

        private void LoadJavaContent(ObservableCollection<LessonItem> lessons)
        {
            var javaLessons = new List<(int number, string title, string description)>
            {
                (1, "Introduction to Java", ""),
                (2, "Variables and Data Types", ""),
                (3, "Control Flow", ""),
                (4, "Methods", ""),
                (5, "Object-Oriented Programming", ""),
                (6, "Arrays and Collections", ""),
                (7, "Exception Handling", ""),
                (8, "File I/O", "")
            };

            foreach (var lesson in javaLessons)
            {
                lessons.Add(new LessonItem
                {
                    Number = lesson.number,
                    Title = lesson.title,
                    Description = lesson.description,
                    Language = "Java",
                    Type = "Lesson"
                });
            }
        }

        private string GetLessonContent(LessonItem lesson)
        {
            if (lesson == null) return string.Empty;

            switch (lesson.Title)
            {
                case "Introduction to C#":
                    return @"# Introduction to C#

## What is C#?
C# is a modern, object-oriented programming language developed by Microsoft. It runs on the .NET platform and can be used to create:
- Windows applications
- Web applications
- Mobile applications
- Games
- Enterprise software

## Key Features
1. Simple and modern syntax
2. Type-safe and structured
3. Object-oriented
4. Component-oriented
5. Unified type system

## Your First C# Program
```csharp
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(""Hello, World!"");
    }
}
```

## Basic Concepts
- Statements end with semicolons
- Code blocks are enclosed in curly braces
- Case-sensitive language
- Rich standard library";

                case "Variables and Data Types":
                    return @"# Variables and Data Types

## Common Data Types
1. Numeric Types:
   - int (whole numbers)
   - double (decimal numbers)
   - decimal (precise decimals)

2. Text Types:
   - char (single character)
   - string (text)

3. Other Types:
   - bool (true/false)
   - DateTime (dates and times)

## Example Usage
```csharp
int age = 25;
string name = ""John"";
bool isStudent = true;
double price = 19.99;
```

## Type Conversion
- Implicit conversion
- Explicit casting
- Convert class methods";

                case "Control Structures":
                    return @"# Control Structures

## If Statements
```csharp
if (condition)
{
    // code
}
else if (another condition)
{
    // code
}
else
{
    // code
}
```

## Loops
1. For Loop
```csharp
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(i);
}
```

2. While Loop
```csharp
while (condition)
{
    // code
}
```

3. Do-While Loop
```csharp
do
{
    // code
} while (condition);
```";

                case "Methods and Functions":
                    return @"# Methods and Functions

## Method Declaration
```csharp
public returnType MethodName(parameters)
{
    // method body
    return value;
}
```

## Example Methods
```csharp
public int Add(int a, int b)
{
    return a + b;
}

public void PrintMessage(string message)
{
    Console.WriteLine(message);
}
```

## Method Parameters
- Required parameters
- Optional parameters
- Named arguments
- Method overloading";

                case "Object-Oriented Programming":
                    return @"# Object-Oriented Programming

## Classes and Objects
```csharp
public class Car
{
    public string Model { get; set; }
    public int Year { get; set; }

    public void StartEngine()
    {
        Console.WriteLine(""Engine started!"");
    }
}
```

## Inheritance
```csharp
public class ElectricCar : Car
{
    public int BatteryLevel { get; set; }
}
```

## Encapsulation
- Private fields
- Public properties
- Protected members";

                case "Collections and Arrays":
                    return @"# Collections and Arrays

## Arrays
```csharp
int[] numbers = new int[5];
string[] names = { ""John"", ""Jane"", ""Bob"" };
```

## Lists
```csharp
List<string> cities = new List<string>();
cities.Add(""New York"");
cities.Add(""London"");
```

## Dictionaries
```csharp
Dictionary<string, int> ages = new Dictionary<string, int>();
ages[""John""] = 25;
ages[""Jane""] = 30;
```";

                case "Exception Handling":
                    return @"# Exception Handling

## Try-Catch Blocks
```csharp
try
{
    // Code that might throw an exception
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Console.WriteLine(""Cannot divide by zero!"");
}
catch (Exception ex)
{
    Console.WriteLine(""An error occurred!"");
}
finally
{
    // Always executed
}
```

## Custom Exceptions
```csharp
public class CustomException : Exception
{
    public CustomException(string message)
        : base(message)
    {
    }
}
```";

                case "File Operations":
                    return @"# File Operations

## Reading Files
```csharp
string text = File.ReadAllText(""file.txt"");
string[] lines = File.ReadAllLines(""file.txt"");
```

## Writing Files
```csharp
File.WriteAllText(""file.txt"", ""Hello World"");
File.WriteAllLines(""file.txt"", new[] { ""Line 1"", ""Line 2"" });
```

## File Streams
```csharp
using (StreamReader reader = new StreamReader(""file.txt""))
{
    string line;
    while ((line = reader.ReadLine()) != null)
    {
        Console.WriteLine(line);
    }
}
```";

                default:
                    return "Content for this lesson is being developed.";
            }
        }

        private void LoadVideoContent(LessonItem lesson)
        {
            if (lesson == null) return;
            
            // Create a new video item based on the lesson
            CurrentVideo = new LessonItem
            {
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl
            };
        }

        [RelayCommand]
        private void PlayVideo()
        {
            if (CurrentVideo != null)
            {
                IsPlaying = true;
                
                // When video finishes playing, mark the lesson as completed
                if (SelectedLesson != null)
                {
                    MarkLessonAsCompleted(SelectedLesson);
                }
            }
        }

        [RelayCommand]
        private void PauseVideo()
        {
            IsPlaying = false;
        }

        private void LoadQuestions(LessonItem lesson)
        {
            CurrentQuestions.Clear();
            if (lesson == null) return;

            // Create a new question based on the lesson content
            var question = new LessonItem
            {
                Title = $"What is the main concept of {lesson.Title}?",
                Options = new List<string>()
            };

            if (SelectedLanguage == "C#")
            {
                switch (lesson.Title)
                {
                    case "Introduction to C#":
                        question.Options = new List<string>
                        {
                            "A programming language developed by Microsoft",
                            "A database management system",
                            "A web browser",
                            "An operating system"
                        };
                        question.CorrectAnswer = "A programming language developed by Microsoft";
                        break;

                    case "Variables and Data Types":
                        question.Options = new List<string>
                        {
                            "int, float, string, bool are basic data types in C#",
                            "All numbers in C# are stored as text",
                            "C# only supports whole numbers",
                            "Variables cannot change their type after declaration"
                        };
                        question.CorrectAnswer = "int, float, string, bool are basic data types in C#";
                        break;

                    // Add more cases for other C# lessons
                    default:
                        question.Options = new List<string>
                        {
                            $"Understanding {lesson.Title} concepts",
                            "Not related to programming",
                            "Only used in web development",
                            "Only used in game development"
                        };
                        question.CorrectAnswer = $"Understanding {lesson.Title} concepts";
                        break;
                }
            }
            else // Java
            {
                switch (lesson.Title)
                {
                    case "Introduction to Java":
                        question.Options = new List<string>
                        {
                            "A platform-independent programming language",
                            "A database system",
                            "A web server",
                            "An IDE"
                        };
                        question.CorrectAnswer = "A platform-independent programming language";
                        break;

                    case "Variables and Data Types":
                        question.Options = new List<string>
                        {
                            "Java has primitive and reference data types",
                            "Java only supports text data",
                            "Variables cannot be modified after creation",
                            "All Java variables are global"
                        };
                        question.CorrectAnswer = "Java has primitive and reference data types";
                        break;

                    // Add more cases for other Java lessons
                    default:
                        question.Options = new List<string>
                        {
                            $"Understanding {lesson.Title} in Java",
                            "Not related to programming",
                            "Only used in Android development",
                            "Only used in web development"
                        };
                        question.CorrectAnswer = $"Understanding {lesson.Title} in Java";
                        break;
                }
            }

            CurrentQuestions.Add(question);
        }

        private void LoadLessonCompletionStatus(ObservableCollection<LessonItem> lessons)
        {
            // First lesson is always unlocked
            if (lessons.Count > 0)
            {
                lessons[0].IsLocked = false;
                
                // If no lessons are completed yet, still show first lesson as unlocked
                var key = $"Lesson_{lessons[0].Language}_{lessons[0].Number}_Completed";
                lessons[0].IsCompleted = Preferences.Default.Get(key, false);
                lessons[0].OnPropertyChanged("StatusIcon");
            }

            // Load completion status from preferences and set lock status
            for (int i = 1; i < lessons.Count; i++)
            {
                var lesson = lessons[i];
                var key = $"Lesson_{lesson.Language}_{lesson.Number}_Completed";
                lesson.IsCompleted = Preferences.Default.Get(key, false);

                // Lock/unlock lessons based on previous lesson completion
                lesson.IsLocked = !lessons[i - 1].IsCompleted;
                
                // Trigger property change for status icon
                lesson.OnPropertyChanged("StatusIcon");
            }
        }

        public void MarkLessonAsCompleted(LessonItem lesson)
        {
            if (lesson == null) return;

            try
            {
                // Mark the lesson as completed
                lesson.IsCompleted = true;
                var key = $"Lesson_{lesson.Language}_{lesson.Number}_Completed";
                Preferences.Default.Set(key, true);

                // Unlock the next lesson if it exists
                var section = LessonSections.FirstOrDefault(s => s.Lessons.Contains(lesson));
                if (section != null)
                {
                    var currentIndex = section.Lessons.IndexOf(lesson);
                    if (currentIndex < section.Lessons.Count - 1)
                    {
                        var nextLesson = section.Lessons[currentIndex + 1];
                        nextLesson.IsLocked = false;
                        nextLesson.OnPropertyChanged("StatusIcon");
                    }
                }

                // Update the status icon
                lesson.OnPropertyChanged("StatusIcon");
                Debug.WriteLine($"Marked lesson {lesson.Title} as completed and unlocked next lesson");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking lesson as completed: {ex.Message}");
            }
        }

        public async Task HandleWrongAnswer()
        {
            if (Lives > 0)
            {
                Lives--;
                
                if (Lives <= 0)
                {
                    // Check if the restart action is allowed based on cooldown
                    if (!CanPerformAction())
                    {
                        // If action was performed too recently, still reduce lives but don't restart
                        return;
                    }
                    
                    // Update the timestamp
                    UpdateActionTimestamp();
                    
                    // When lives are depleted, restart the lesson with full hearts
                    await RestartCurrentLessonAsync();
                }
            }
        }
        
        // Method to handle when time runs out
        public async Task HandleTimeOut()
        {
            // Check if the action is allowed based on cooldown
            if (!CanPerformAction())
            {
                // If action was performed too recently, ignore
                return;
            }
            
            // Update the timestamp
            UpdateActionTimestamp();
            
            // No Game Over alert - just restart silently
            
            // Reset lives and restart current lesson
            await RestartCurrentLessonAsync();
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            try
            {
                if (_isInitialized)
                {
                    LoadContent();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling language change: {ex.Message}");
            }
        }
    }
} 