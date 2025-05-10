using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class PlayViewModel : INotifyPropertyChanged
    {
        private double _heart1Scale = 1.0;
        private double _heart2Scale = 1.0;
        private double _heart3Scale = 1.0;
        private int _lives = 3;
        private int _score = 0;
        private int _highScore = 0;
        private int _timeRemaining = 30;
        private string _currentQuestion = "";
        private bool _isGameOver = false;
        private ObservableCollection<string> _answerOptions;
        private string _correctAnswer = "";
        private IDispatcherTimer _timer;
        private bool _isWrongAnswer = false;
        private string _lastWrongAnswer = "";
        private string _feedbackMessage = "";
        private bool _isHeartAnimating;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double Heart1Scale
        {
            get => _heart1Scale;
            set
            {
                _heart1Scale = value;
                OnPropertyChanged();
            }
        }

        public double Heart2Scale
        {
            get => _heart2Scale;
            set
            {
                _heart2Scale = value;
                OnPropertyChanged();
            }
        }

        public double Heart3Scale
        {
            get => _heart3Scale;
            set
            {
                _heart3Scale = value;
                OnPropertyChanged();
            }
        }

        public int Lives
        {
            get => _lives;
            set
            {
                if (_lives != value)
                {
                    _lives = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged();
                    if (_score > HighScore)
                    {
                        HighScore = _score;
                    }
                }
            }
        }

        public int HighScore
        {
            get => _highScore;
            set
            {
                if (_highScore != value)
                {
                    _highScore = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (_timeRemaining != value)
                {
                    _timeRemaining = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                if (_currentQuestion != value)
                {
                    _currentQuestion = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                if (_isGameOver != value)
                {
                    _isGameOver = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> AnswerOptions
        {
            get => _answerOptions;
            set
            {
                _answerOptions = value;
                OnPropertyChanged();
            }
        }

        public bool IsWrongAnswer
        {
            get => _isWrongAnswer;
            set
            {
                if (_isWrongAnswer != value)
                {
                    _isWrongAnswer = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastWrongAnswer
        {
            get => _lastWrongAnswer;
            set
            {
                if (_lastWrongAnswer != value)
                {
                    _lastWrongAnswer = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FeedbackMessage
        {
            get => _feedbackMessage;
            set
            {
                if (_feedbackMessage != value)
                {
                    _feedbackMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsHeartAnimating
        {
            get => _isHeartAnimating;
            set
            {
                _isHeartAnimating = value;
                OnPropertyChanged();
            }
        }

        public ICommand RestartCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand CheckAnswerCommand { get; private set; }

        public PlayViewModel()
        {
            _answerOptions = new ObservableCollection<string>();
            RestartCommand = new Command(RestartGame);
            CloseCommand = new Command(CloseGame);
            CheckAnswerCommand = new Command<string>(CheckAnswer);
            
            // Load saved high score
            HighScore = Preferences.Default.Get("HighScore", 0);
            
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            InitializeGame();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
            }
            else
            {
                if (Lives > 0)
                {
                    Lives--;
                    if (Lives <= 0)
                    {
                        EndGame();
                    }
                    else
                    {
                        LoadNextQuestion();
                        TimeRemaining = 30; // Reset timer for next question
                    }
                }
            }
        }

        private void InitializeGame()
        {
            // Reset all game state
            Lives = 3;
            Score = 0;
            TimeRemaining = 30;
            IsGameOver = false;
            IsWrongAnswer = false;
            LastWrongAnswer = "";
            FeedbackMessage = "";
            
            // Explicitly reset heart scales and visibility
            Heart1Scale = 1.0;
            Heart2Scale = 1.0;
            Heart3Scale = 1.0;
            IsHeartAnimating = false;
            
            // Force property change notification for Lives to update heart visibility
            OnPropertyChanged(nameof(Lives));
            
            // Load first question
            LoadNextQuestion();
            
            // Start the timer
            _timer.Start();
        }

        private void LoadNextQuestion()
        {
            LastWrongAnswer = "";
            FeedbackMessage = "";
            IsWrongAnswer = false;
            
            // Reset timer for new question
            TimeRemaining = 30;

            // Example question (you'll need to implement your own question bank)
            var questions = new List<(string Question, string[] Options, string Answer)>
            {
                ("Which collection in C# allows key-value pairs?", 
                 new[] { "LIST<T>", "DICTIONARY<K,V>", "ARRAY[]" }, 
                 "DICTIONARY<K,V>"),
                ("What is the base class for all classes in C#?", 
                 new[] { "Object", "Base", "Class" }, 
                 "Object"),
                ("Which keyword is used to prevent inheritance in C#?", 
                 new[] { "static", "sealed", "final" }, 
                 "sealed"),
            };

            var random = new Random();
            var questionIndex = random.Next(questions.Count);
            var selectedQuestion = questions[questionIndex];

            CurrentQuestion = selectedQuestion.Question;
            _correctAnswer = selectedQuestion.Answer;

            AnswerOptions.Clear();
            foreach (var option in selectedQuestion.Options)
            {
                AnswerOptions.Add(option);
            }
        }

        private async void CheckAnswer(string answer)
        {
            // Prevent multiple answers while processing
            if (IsWrongAnswer) return;

            if (answer == _correctAnswer)
            {
                IsWrongAnswer = false;  // This will make the feedback green
                Score += 1;
                FeedbackMessage = "Correct Answer";
                await Task.Delay(1000); // Show feedback for 1 second
                LoadNextQuestion();
            }
            else
            {
                IsWrongAnswer = true;  // This will make the feedback red
                LastWrongAnswer = answer;
                FeedbackMessage = "Wrong Answer";
                
                // Decrease lives immediately and wait for animation
                await HandleWrongAnswer();
                
                // Keep wrong answer feedback visible for a moment before resetting
                await Task.Delay(1000);
                IsWrongAnswer = false;
            }
        }

        private async Task HandleWrongAnswer()
        {
            if (Lives > 0)
            {
                Lives--;  // This will trigger the animation in PlayPage.xaml.cs
                
                await Task.Delay(1000); // Show feedback message for 1 second
                
                if (Lives <= 0)
                {
                    EndGame();
                    SaveHighScore();
                }
                else
                {
                    // Reset timer and load next question
                    TimeRemaining = 30;
                    LoadNextQuestion();
                }
            }
        }

        private void RestartGame()
        {
            _timer.Stop(); // Stop the current timer
            
            // Reset all game state
            Lives = 3;
            Score = 0;
            TimeRemaining = 30;
            IsGameOver = false;
            IsWrongAnswer = false;
            LastWrongAnswer = "";
            FeedbackMessage = "";
            
            // Explicitly reset heart scales and visibility
            Heart1Scale = 1.0;
            Heart2Scale = 1.0;
            Heart3Scale = 1.0;
            IsHeartAnimating = false;
            
            // Force property change notification for Lives to update heart visibility
            OnPropertyChanged(nameof(Lives));
            
            // Load new question and start timer
            LoadNextQuestion();
            _timer.Start();
        }

        private void CloseGame()
        {
            _timer.Stop();
            Application.Current.MainPage.Navigation.PopModalAsync();
        }

        private void EndGame()
        {
            _timer.Stop();
            IsGameOver = true;
            SaveHighScore();
        }

        private void SaveHighScore()
        {
            var currentHighScore = Preferences.Default.Get("HighScore", 0);
            if (Score > currentHighScore)
            {
                Preferences.Default.Set("HighScore", Score);
                HighScore = Score;
            }
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