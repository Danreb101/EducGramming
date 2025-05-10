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

    public class LessonItem
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public partial class LessonViewModel : ObservableObject
    {
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
        private bool _isSidebarVisible = false;

        [ObservableProperty]
        private double _sidebarWidth = 300;

        [ObservableProperty]
        private string _lessonContent;

        [ObservableProperty]
        private bool _isPlaying;

        public LessonViewModel()
        {
            LoadContent();
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarVisible = !IsSidebarVisible;
        }

        [RelayCommand]
        private void ToggleSection(LessonSection section)
        {
            if (section != null)
            {
                section.IsExpanded = !section.IsExpanded;
            }
        }

        [RelayCommand]
        private void SelectLesson(LessonItem lesson)
        {
            try
            {
                if (lesson == null) return;
                // Update selection state
                foreach (var section in LessonSections)
                {
                    foreach (var item in section.Lessons)
                    {
                        item.IsSelected = item == lesson;
                    }
                }
                SelectedLesson = lesson;
                // Update lesson content based on selection
                LessonContent = GetLessonContent(lesson);
                // Load video content
                LoadVideoContent(lesson);
                // Load corresponding questions
                LoadQuestions(lesson);
                // Do NOT close the sidebar here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error selecting lesson: {ex.Message}");
                // Handle the error gracefully - could show a message to the user if needed
            }
        }

        private void LoadContent()
        {
            LessonSections.Clear();

            var csharpSection = new LessonSection 
            { 
                Title = "C# Lessons",
                IsExpanded = true
            };
            LoadCSharpContent(csharpSection.Lessons);
            LessonSections.Add(csharpSection);

            var javaSection = new LessonSection 
            { 
                Title = "Java Lessons",
                IsExpanded = false
            };
            LoadJavaContent(javaSection.Lessons);
            LessonSections.Add(javaSection);

            // Select first lesson by default
            if (csharpSection.Lessons.Count > 0)
            {
                SelectedLesson = csharpSection.Lessons[0];
            }
        }

        private void LoadCSharpContent(ObservableCollection<LessonItem> lessons)
        {
            var csharpLessons = new List<(int number, string title, string description, List<string> options, string correctAnswer)>
            {
                (1, "Introduction to C#", 
                @"Learn the basics of C# programming:
                - What is C# and .NET Framework
                - Your first C# program
                - Basic syntax and structure
                - Console input and output
                - Comments and documentation", 
                new List<string> {
                    "C# is a programming language developed by Microsoft",
                    "C# is a database management system",
                    "C# is a web browser",
                    "C# is an operating system"
                },
                "C# is a programming language developed by Microsoft"),

                (2, "Variables and Data Types", 
                @"Understanding data types and variables:
                - Integer types (int, long, short)
                - Floating-point types (float, double, decimal)
                - Character and string types
                - Boolean type
                - Variable declaration and initialization
                - Type conversion and casting", 
                new List<string> {
                    "int, float, string, bool are basic data types in C#",
                    "All numbers in C# are stored as text",
                    "C# only supports whole numbers",
                    "Variables cannot change their type after declaration"
                },
                "int, float, string, bool are basic data types in C#"),

                (3, "Control Structures", 
                @"Learn about control flow:
                - If-else statements
                - Switch statements
                - For loops
                - While and do-while loops
                - Break and continue statements
                - Logical operators", 
                new List<string> {
                    "Control structures help manage program flow",
                    "Control structures are only used for mathematical operations",
                    "Loops can only run 10 times maximum",
                    "If statements can only check equality"
                },
                "Control structures help manage program flow"),

                (4, "Methods and Functions", 
                @"Working with methods:
                - Method declaration and definition
                - Parameters and return types
                - Method overloading
                - Optional parameters
                - Named arguments
                - Expression-bodied methods", 
                new List<string> {
                    "Methods are reusable blocks of code that perform specific tasks",
                    "Methods can only return void",
                    "Methods cannot take parameters",
                    "Every method must be static"
                },
                "Methods are reusable blocks of code that perform specific tasks"),

                (5, "Object-Oriented Programming", 
                @"Understanding OOP concepts:
                - Classes and objects
                - Properties and fields
                - Constructors
                - Inheritance
                - Polymorphism
                - Encapsulation
                - Interfaces and abstract classes", 
                new List<string> {
                    "OOP helps organize code using classes and objects",
                    "OOP is only used for game development",
                    "Classes cannot inherit from other classes",
                    "Objects cannot have properties"
                },
                "OOP helps organize code using classes and objects"),

                (6, "Collections and Arrays", 
                @"Working with data collections:
                - Arrays and array operations
                - Lists and List<T>
                - Dictionaries and Dictionary<TKey,TValue>
                - HashSet and SortedSet
                - Queue and Stack
                - LINQ basics", 
                new List<string> {
                    "Collections are used to store multiple values",
                    "Arrays can only store numbers",
                    "Lists cannot be modified after creation",
                    "Dictionaries can only use strings as keys"
                },
                "Collections are used to store multiple values"),

                (7, "Exception Handling", 
                @"Managing errors and exceptions:
                - Try-catch blocks
                - Multiple catch blocks
                - Finally block
                - Throwing exceptions
                - Custom exceptions
                - Best practices for error handling", 
                new List<string> {
                    "Exception handling helps manage runtime errors",
                    "Exceptions automatically fix all errors",
                    "Try-catch blocks slow down the program",
                    "Only system exceptions can be caught"
                },
                "Exception handling helps manage runtime errors"),

                (8, "File Operations", 
                @"Working with files and streams:
                - Reading text files
                - Writing to files
                - File and Directory classes
                - Stream operations
                - File system operations
                - Async file operations", 
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
                (1, "Introduction to Java", "Learn the basics of Java programming including variables, data types, and control structures."),
                (2, "Variables and Data Types", "Understanding different data types and how to use variables in Java."),
                (3, "Control Flow", "Learn about if statements, loops, and switch cases in Java."),
                (4, "Methods", "Creating and using methods, understanding parameters and return values."),
                (5, "Object-Oriented Programming", "Understanding classes, objects, inheritance, and polymorphism in Java."),
                (6, "Arrays and Collections", "Working with arrays, ArrayLists, and other collection types."),
                (7, "Exception Handling", "Learn how to handle errors and exceptions in your code."),
                (8, "File I/O", "Reading from and writing to files in Java.")
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

        partial void OnSelectedLanguageChanged(string value)
        {
            try
            {
                // Clear current state
                SelectedLesson = null;
                CurrentVideo = null;
                CurrentQuestions.Clear();
                LessonContent = string.Empty;
                IsPlaying = false;

                // Reload content for the new language
            LoadContent();

                // Select first lesson by default
                var firstSection = LessonSections.FirstOrDefault();
                if (firstSection != null && firstSection.Lessons.Count > 0)
                {
                    SelectLesson(firstSection.Lessons[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing language: {ex.Message}");
            }
        }
    }
} 