using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using System.Windows.Input;
using EducGramming.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EducGramming.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private int _lessonsCompleted;
        public int LessonsCompleted
        {
            get => _lessonsCompleted;
            set
            {
                if (_lessonsCompleted != value)
                {
                    _lessonsCompleted = value;
                    OnPropertyChanged(nameof(LessonsCompleted));
                }
            }
        }

        private int _highScore;
        public int HighScore
        {
            get => _highScore;
            set
            {
                if (_highScore != value)
                {
                    _highScore = value;
                    OnPropertyChanged(nameof(HighScore));
                }
            }
        }

        private int _timeSpent;
        public int TimeSpent
        {
            get => _timeSpent;
            set
            {
                if (_timeSpent != value)
                {
                    _timeSpent = value;
                    OnPropertyChanged(nameof(TimeSpent));
                }
            }
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        private string _accountToDelete;
        public string AccountToDelete
        {
            get => _accountToDelete;
            set
            {
                if (_accountToDelete != value)
                {
                    _accountToDelete = value;
                    OnPropertyChanged(nameof(AccountToDelete));
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ShowAdminCommand { get; }

        private readonly UserService _userService;
        private FirebaseAuthService _firebaseAuthService;
        private int _adminTapCount = 0;

        public ProfileViewModel()
        {
            _userService = new UserService();
            _firebaseAuthService = new FirebaseAuthService();
            
            // Load user profile data
            LoadUserProfile();
            
            // Initialize commands
            LogoutCommand = new Command(OnLogout);
            ChangePasswordCommand = new Command(OnChangePassword);
            DeleteAccountCommand = new Command<string>(OnDeleteAccount);
            ShowAdminCommand = new Command(OnShowAdmin);
            
            // Initialize properties
            IsAdmin = false;
            AccountToDelete = string.Empty;
            
            Debug.WriteLine("ProfileViewModel initialized");
        }

        private async void LoadUserProfile()
        {
            try
            {
                Debug.WriteLine("Loading user profile data");
                
                // Get current email from preferences (always available after login)
                Email = Preferences.Default.Get<string>("CurrentEmail", "user@example.com");
                
                // Try to get profile data from preferences first (faster)
                Username = Preferences.Default.Get<string>("CurrentUsername", "User");
                FullName = Preferences.Default.Get<string>("CurrentFullName", Username);
                
                Debug.WriteLine($"Loaded profile from preferences: Email={Email}, Username={Username}, FullName={FullName}");
                
                // Check if this is an admin account
                IsAdmin = Email.ToLower().Contains("admin") || Email.ToLower() == "test123@gmail.com";
                
                // Try to get more complete profile from user service
                var userProfile = await _userService.GetCurrentUserProfile();
                if (userProfile != null)
                {
                    Username = userProfile.Username;
                    FullName = userProfile.FullName;
                    Email = userProfile.Email;
                    
                    Debug.WriteLine($"Updated profile from user service: Username={Username}, FullName={FullName}");
                    
                    // Also update preferences for future use
                    Preferences.Default.Set("CurrentUsername", Username);
                    Preferences.Default.Set("CurrentFullName", FullName);
                }
                
                // Load the user statistics - for now use dummy data
                LessonsCompleted = Preferences.Default.Get<int>("LessonsCompleted", 0);
                HighScore = Preferences.Default.Get<int>("HighScore", 0);
                TimeSpent = Preferences.Default.Get<int>("TimeSpent", 0);
                
                // Important: Ensure user exists in Firebase
                // This helps solve the issue with users not appearing in Firebase
                if (!string.IsNullOrEmpty(Email) && !Email.ToLower().Contains("test"))
                {
                    // Run this in the background to avoid blocking the UI
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Small delay to ensure UI is loaded first
                            await Task.Delay(1000);
                            
                            Debug.WriteLine($"Starting Firebase migration check for {Email}");
                            await _userService.MigrateExistingUsersToFirebase();
                        }
                        catch (Exception migrationEx)
                        {
                            Debug.WriteLine($"Error during Firebase migration: {migrationEx.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
                // Set defaults
                Username = "User";
                FullName = "User";
                Email = "user@example.com";
            }
        }

        private async void OnLogout()
        {
            try
            {
                // Clear user preferences
                Preferences.Default.Remove("CurrentEmail");
                Preferences.Default.Remove("CurrentUsername");
                Preferences.Default.Remove("CurrentFullName");
                Preferences.Default.Remove("StaySignedIn");
                
                // Navigate back to login
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current.MainPage = new LoginShell();
                });
                
                Debug.WriteLine("User logged out successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        private async void OnChangePassword()
        {
            try
        {
            // Navigate to change password page
            await Shell.Current.GoToAsync("//changepassword");
        }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to change password: {ex.Message}");
            }
        }
        
        private void OnShowAdmin()
        {
            _adminTapCount++;
            
            if (_adminTapCount >= 5)
            {
                _adminTapCount = 0;
                IsAdmin = true;
                OnPropertyChanged(nameof(IsAdmin));
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Admin Mode", "Admin features activated", "OK");
                });
            }
        }
        
        private async void OnDeleteAccount(string emailToDelete)
        {
            if (string.IsNullOrWhiteSpace(emailToDelete))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter an email address to delete", "OK");
                return;
            }
            
            try
            {
                // Ask for confirmation
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Deletion", 
                    $"Are you sure you want to delete the account: {emailToDelete}?", 
                    "Yes", "No");

                if (!confirm) return;
                
                // Ask for password for the account to delete
                string password = await Application.Current.MainPage.DisplayPromptAsync(
                    "Authentication Required", 
                    "Enter the password for this account:", 
                    "Delete", "Cancel", 
                    "Password", 
                    -1, 
                    Keyboard.Default, 
                    "");
                    
                if (string.IsNullOrEmpty(password)) return;
                
                // Try to delete the account
                bool success = await _firebaseAuthService.DeleteUserAccount(emailToDelete, password);
                
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", $"Account {emailToDelete} was deleted successfully", "OK");
                    AccountToDelete = string.Empty;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete account", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete account: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
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