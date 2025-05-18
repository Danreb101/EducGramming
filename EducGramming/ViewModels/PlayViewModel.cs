using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

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
        private readonly IAudioManager _audioManager;
        private IAudioPlayer? _correctSoundPlayer;
        private IAudioPlayer? _wrongSoundPlayer;
        private IAudioPlayer? _failSoundPlayer;
        private List<(string Language, string Question, string[] Options, string Answer)> _allQuestions;
        private List<int> _remainingQuestionIndices;
        private int _currentQuestionIndex = -1;
        private string _currentLanguage;
        private bool _isProcessingAnswer = false;
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set { _currentLanguage = value; OnPropertyChanged(); }
        }
        private Color _currentLanguageColor;
        public Color CurrentLanguageColor
        {
            get => _currentLanguageColor;
            set { _currentLanguageColor = value; OnPropertyChanged(); }
        }
        private string _droppedAnswer;
        public string DroppedAnswer
        {
            get => _droppedAnswer;
            set { _droppedAnswer = value; OnPropertyChanged(); }
        }
        private bool _feedbackVisible;
        public bool FeedbackVisible
        {
            get => _feedbackVisible;
            set { _feedbackVisible = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action HeartFadeRequested;

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

        public PlayViewModel(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _answerOptions = new ObservableCollection<string>();
            RestartCommand = new Command(RestartFromGameOver);
            CloseCommand = new Command(CloseGame);
            CheckAnswerCommand = new Command<string>(CheckAnswer);
            
            // Load saved high score
            HighScore = Preferences.Default.Get("HighScore", 0);
            
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            InitializeQuestionBank();
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
                // Time is up, treat as game over
                EndGame();
                SaveHighScore();
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

        private void InitializeQuestionBank()
        {
            _allQuestions = new List<(string, string, string[], string)>
            {
                ("C#", "Which keyword is used to define a method that can be overridden in a derived class?",
                    new[] { "static", "virtual", "override", "sealed" }, "virtual"),
                ("Java", "You want to make a method accessible without creating an instance. What keyword should you use?",
                    new[] { "final", "private", "static", "protected" }, "static"),
                ("C#", "How do you write a comment that spans only one line in C#?",
                    new[] { "// This is a comment", "# This is a comment", "/* This is a comment */", "-- This is a comment" }, "// This is a comment"),
                ("Java", "Which exception is thrown when a null object is accessed in Java?",
                    new[] { "NullPointerException", "IOException", "ArrayIndexOutOfBoundsException", "RuntimeException" }, "NullPointerException"),
                ("Java", "What is the correct way to declare a main method in Java?",
                    new[] { "public static void main(String[] args)", "void main(String[] args)", "static void Main(string[] args)", "Main(String[] args)" }, "public static void main(String[] args)"),
            };
            ShuffleQuestions();
        }

        private void ShuffleQuestions()
        {
            var indices = Enumerable.Range(0, _allQuestions.Count).ToList();
            var rng = new Random();
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }
            _remainingQuestionIndices = indices;
        }

        private void LoadNextQuestion()
        {
            LastWrongAnswer = "";
            FeedbackMessage = "";
            FeedbackVisible = false;
            IsWrongAnswer = false;
            DroppedAnswer = null;
            TimeRemaining = 30;
            if (_remainingQuestionIndices == null || _remainingQuestionIndices.Count == 0)
            {
                ShuffleQuestions();
            }
            if (_currentQuestionIndex != -1 && _remainingQuestionIndices.Contains(_currentQuestionIndex))
                _remainingQuestionIndices.Remove(_currentQuestionIndex);
            if (_remainingQuestionIndices.Count == 0)
            {
                ShuffleQuestions();
            }
            _currentQuestionIndex = _remainingQuestionIndices[0];
            _remainingQuestionIndices.RemoveAt(0);
            var selectedQuestion = _allQuestions[_currentQuestionIndex];
            CurrentLanguage = selectedQuestion.Language;
            CurrentLanguageColor = selectedQuestion.Language == "C#" ? Color.FromArgb("#FFD600") : Color.FromArgb("#2979FF");
            CurrentQuestion = selectedQuestion.Question;
            _correctAnswer = selectedQuestion.Answer;
            AnswerOptions.Clear();
            foreach (var option in selectedQuestion.Options)
            {
                AnswerOptions.Add(option);
            }
        }

        public void CheckAnswer(string answer)
        {
            if (IsGameOver) return;
            if (_isProcessingAnswer) return; // Prevent double-processing
            _isProcessingAnswer = true;

            DroppedAnswer = answer;
            if (answer == _correctAnswer)
            {
                IsWrongAnswer = false;
                FeedbackMessage = "Correct Answer";
                FeedbackVisible = true;
                PlayCorrectSound();
                Score += 1;
                Device.StartTimer(TimeSpan.FromMilliseconds(900), () => {
                    FeedbackVisible = false;
                    _isProcessingAnswer = false;
                    LoadNextQuestion();
                    return false;
                });
            }
            else
            {
                IsWrongAnswer = true;
                LastWrongAnswer = answer;
                FeedbackMessage = "Wrong Answer";
                FeedbackVisible = true;
                PlayWrongSound();
                if (_currentQuestionIndex != -1 && _remainingQuestionIndices.Contains(_currentQuestionIndex))
                    _remainingQuestionIndices.Remove(_currentQuestionIndex);
                Lives--;
                if (Lives <= 0)
                {
                    Device.StartTimer(TimeSpan.FromMilliseconds(600), () => {
                        IsGameOver = true;
                        PlayFailSound();
                        SaveHighScore();
                        _isProcessingAnswer = false;
                        return false;
                    });
                    return;
                }
                // Show feedback for a very short time, then load next question
                Device.StartTimer(TimeSpan.FromMilliseconds(200), () => {
                    FeedbackVisible = false;
                    IsWrongAnswer = false;
                    _isProcessingAnswer = false;
                    LoadNextQuestion();
                    return false;
                });
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
            PlayFailSound();
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

        public void HandleWrongAnswer()
        {
            // This method is now deprecated and functionality is moved to CheckAnswerAsync
            // Keeping minimal implementation for backward compatibility
            if (Lives > 0 && Lives <= 3)
            {
                // Lives decrement is now handled by CheckAnswerAsync
            }
        }

        public bool CheckAnswerSync(string answer)
        {
            if (IsWrongAnswer) return false;
            bool isCorrect = answer == _correctAnswer;
            if (isCorrect)
            {
                IsWrongAnswer = false;
                Score += 1;
                FeedbackMessage = "Correct Answer";
                PlayCorrectSound();
                LoadNextQuestion();
            }
            else
            {
                IsWrongAnswer = true;
                LastWrongAnswer = answer;
                FeedbackMessage = "Wrong Answer";
                PlayWrongSound();
                if (_currentQuestionIndex != -1 && _remainingQuestionIndices.Contains(_currentQuestionIndex))
                    _remainingQuestionIndices.Remove(_currentQuestionIndex);
                if (Lives > 0)
                {
                    Lives--;
                    if (Lives <= 0)
                    {
                        EndGame();
                        SaveHighScore();
                    }
                    else
                    {
                        LoadNextQuestion();
                    }
                }
            }
            return isCorrect;
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

        private void PlayCorrectSound()
        {
            try
            {
                _correctSoundPlayer?.Stop();
                _correctSoundPlayer?.Dispose();
                var file = FileSystem.OpenAppPackageFileAsync("Sounds/Correct Sound Effect  Bgm & Sound Effect.mp3").Result;
                _correctSoundPlayer = _audioManager.CreatePlayer(file);
                _correctSoundPlayer.Play();
            }
            catch { }
        }

        private void PlayWrongSound()
        {
            try
            {
                _wrongSoundPlayer?.Stop();
                _wrongSoundPlayer?.Dispose();
                var file = FileSystem.OpenAppPackageFileAsync("Sounds/Wrong Answer - Sound Effects HQ.mp3").Result;
                _wrongSoundPlayer = _audioManager.CreatePlayer(file);
                _wrongSoundPlayer.Play();
            }
            catch { }
        }

        private void PlayFailSound()
        {
            try
            {
                _failSoundPlayer?.Stop();
                _failSoundPlayer?.Dispose();
                var file = FileSystem.OpenAppPackageFileAsync("Sounds/Fail Sound Effect.mp3").Result;
                _failSoundPlayer = _audioManager.CreatePlayer(file);
                _failSoundPlayer.Play();
            }
            catch { }
        }

        public void ContinueAfterHeartFade()
        {
            if (!IsWrongAnswer)
                FeedbackVisible = false;
            IsWrongAnswer = false;
            if (Lives == 0)
            {
                EndGame();
            }
            else
            {
                LoadNextQuestion();
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