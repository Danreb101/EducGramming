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
        private bool _isBusy;
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

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
                    OnPropertyChanged(nameof(Lives));
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
            RestartCommand = new Command(RestartFromGameOver);
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
                        EndGame(); // This sets IsGameOver = true to show the overlay
                        SaveHighScore();
                        // No popup dialog, only the Game Over overlay
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
            Score = 0;
            TimeRemaining = 30;
            IsGameOver = false;
            IsWrongAnswer = false;
            LastWrongAnswer = "";
            FeedbackMessage = "";
            IsHeartAnimating = false;
            
            // GUARANTEED RESET TO 3 LIVES - direct field manipulation
            _lives = 3;
            
            // Notify UI explicitly 
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

        private void RestartGame()
        {
            try
            {
                // Stop the timer before doing any UI updates
                _timer?.Stop();
                
                // Reset all game state
                Score = 0;
                TimeRemaining = 30;
                IsGameOver = false;
                IsWrongAnswer = false;
                LastWrongAnswer = "";
                FeedbackMessage = "";
                IsHeartAnimating = false;
                
                // INSTANT RESET: Set lives to 3 with immediate UI update
                _lives = 3;
                OnPropertyChanged(nameof(Lives));
                
                // Immediately load new question and start timer
                LoadNextQuestion();
                _timer?.Start();
            }
            catch (Exception ex)
            {
                // Just log and recover
                System.Diagnostics.Debug.WriteLine($"Error in RestartGame: {ex.Message}");
                
                // Ensure lives are reset
                _lives = 3;
                OnPropertyChanged(nameof(Lives));
            }
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

        // Ensure this method gets called when clicking Play Again on Game Over
        public void RestartFromGameOver()
        {
            try
            {
                // Stop the timer before doing any UI updates
                _timer?.Stop();
                
                // Reset game state
                Score = 0;
                TimeRemaining = 30;
                IsGameOver = false;
                FeedbackMessage = "";
                IsHeartAnimating = false;
                
                // INSTANT RESET: Set lives to 3 with immediate UI update
                _lives = 3;
                OnPropertyChanged(nameof(Lives));
                
                // Immediately load new question and start timer
                LoadNextQuestion();
                _timer?.Start();
            }
            catch (Exception ex)
            {
                // Just log and recover
                System.Diagnostics.Debug.WriteLine($"Error in RestartFromGameOver: {ex.Message}");
                
                // Ensure lives are reset
                _lives = 3;
                OnPropertyChanged(nameof(Lives));
                
                // Ensure timer is running
                _timer?.Start();
            }
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

        public async Task HandleWrongAnswer()
        {
            // This method is now deprecated and functionality is moved to CheckAnswerAsync
            // Keeping minimal implementation for backward compatibility
            if (Lives > 0 && Lives <= 3)
            {
                // Lives decrement is now handled by CheckAnswerAsync
            }
        }

        public async Task<bool> CheckAnswerAsync(string answer)
        {
            try
            {
                if (IsWrongAnswer) return false;

                bool isCorrect = answer == _correctAnswer;
                
                if (isCorrect)
                {
                    IsWrongAnswer = false;
                    Score += 1;
                    FeedbackMessage = "Correct Answer";
                    
                    // Show feedback for a moment before moving to next question
                    await Task.Delay(1000);
                    LoadNextQuestion();
                }
                else
                {
                    IsWrongAnswer = true;
                    LastWrongAnswer = answer;
                    FeedbackMessage = "Wrong Answer";
                    
                    // Show feedback before decreasing lives
                    await Task.Delay(500);
                    
                    if (Lives > 0)
                    {
                        Lives--; // Decrease lives once
                        
                        if (Lives <= 0)
                        {
                            EndGame();
                            SaveHighScore();
                            
                            // REMOVED: Don't show alert popup, use only Game Over overlay
                            // Just restart game when user clicks Play Again button on overlay
                        }
                        else
                        {
                            // Show the feedback for a moment longer before next question
                            await Task.Delay(500);
                            LoadNextQuestion(); 
                        }
                    }
                }
                
                return isCorrect;
            }
            catch (Exception ex)
            {
                // Log error and recover
                System.Diagnostics.Debug.WriteLine($"Error in CheckAnswerAsync: {ex.Message}");
                return false;
            }
        }

        public void ResetLives()
        {
            try
            {
                // Always force reset to 3 lives with no animation
                _lives = 3; // Direct field access to ensure proper value
                
                // Multiple notifications to ensure UI updates
                OnPropertyChanged(nameof(Lives));
                
                // Log the reset for debugging
                System.Diagnostics.Debug.WriteLine("Lives reset to 3");
            }
            catch (Exception ex)
            {
                // Recover from any errors
                System.Diagnostics.Debug.WriteLine($"Error in ResetLives: {ex.Message}");
                _lives = 3;
                OnPropertyChanged(nameof(Lives));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error in PropertyChanged: {ex.Message}");
            }
        }
    }
} 