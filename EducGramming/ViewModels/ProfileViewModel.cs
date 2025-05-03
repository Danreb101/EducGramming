using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace EducGramming.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private string _fullName = string.Empty;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private int _lessonsCompleted;
        private int _highScore;
        private int _timeSpeed;
        private int _streak;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LessonsCompleted
        {
            get => _lessonsCompleted;
            set
            {
                if (_lessonsCompleted != value)
                {
                    _lessonsCompleted = value;
                    OnPropertyChanged();
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

        public int TimeSpeed
        {
            get => _timeSpeed;
            set
            {
                if (_timeSpeed != value)
                {
                    _timeSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Streak
        {
            get => _streak;
            set
            {
                if (_streak != value)
                {
                    _streak = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ChangePasswordCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public ProfileViewModel()
        {
            LoadUserData();
            LoadProgress();
            
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        private void LoadUserData()
        {
            Username = Preferences.Default.Get("CurrentUsername", "Player");
            Email = Preferences.Default.Get("CurrentEmail", string.Empty);
            FullName = Preferences.Default.Get("CurrentFullName", string.Empty);
        }

        private void LoadProgress()
        {
            // TODO: Implement progress tracking service
            LessonsCompleted = 0;
            HighScore = 0;
            TimeSpeed = 0;
            Streak = 0;
        }

        private async Task ChangePasswordAsync()
        {
            // Navigate to change password page
            await Shell.Current.GoToAsync("changepassword");
        }

        private async Task LogoutAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Logout", 
                "Are you sure you want to logout?", 
                "Yes", 
                "No");

            if (confirm)
            {
                // Clear preferences
                Preferences.Default.Remove("CurrentUsername");
                Preferences.Default.Remove("CurrentEmail");
                Preferences.Default.Remove("CurrentFullName");
                
                // Navigate to login
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//login");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 