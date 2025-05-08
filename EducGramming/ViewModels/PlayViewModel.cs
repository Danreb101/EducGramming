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
                Lives = Math.Max(0, Lives - 1);
                if (Lives <= 0)
                {
                    EndGame();
                }
                else
                {
                    LoadNextQuestion();
                }
            }
        }

        private void InitializeGame()
        {
            Lives = 3;
            Score = 0;
            TimeRemaining = 30;
            IsGameOver = false;
            
            // Reset heart scales and animations
            Heart1Scale = 1.0;
            Heart2Scale = 1.0;
            Heart3Scale = 1.0;
            IsHeartAnimating = false;
            
            LoadNextQuestion();
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
                Score += 1;
                FeedbackMessage = "Correct!";
                await Task.Delay(300);
                LoadNextQuestion();
            }
            else
            {
                IsWrongAnswer = true;
                LastWrongAnswer = answer;
                FeedbackMessage = "Wrong Answer!";
                
                // Decrease lives immediately and wait for animation
                await HandleWrongAnswer();
                
                IsWrongAnswer = false;
            }
        }

        private async Task HandleWrongAnswer()
        {
            Lives--;
            await AnimateHearts();
            
            if (Lives <= 0)
            {
                IsGameOver = true;
                SaveHighScore();
            }
            else
            {
                LoadNextQuestion();
            }
        }

        private async Task AnimateHearts()
        {
            // Animate all visible hearts
            var tasks = new List<Task>();
            
            if (Lives >= 0)
            {
                tasks.Add(AnimateHeart(1));
                if (Lives >= 1) tasks.Add(AnimateHeart(2));
                if (Lives >= 2) tasks.Add(AnimateHeart(3));
            }
            
            await Task.WhenAll(tasks);
        }

        private async Task AnimateHeart(int heartNumber)
        {
            switch (heartNumber)
            {
                case 1:
                    Heart1Scale = 1.5;
                    await Task.Delay(100);
                    Heart1Scale = 1.0;
                    break;
                case 2:
                    Heart2Scale = 1.5;
                    await Task.Delay(100);
                    Heart2Scale = 1.0;
                    break;
                case 3:
                    Heart3Scale = 1.5;
                    await Task.Delay(100);
                    Heart3Scale = 1.0;
                    break;
            }
        }

        private void RestartGame()
        {
            InitializeGame();
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