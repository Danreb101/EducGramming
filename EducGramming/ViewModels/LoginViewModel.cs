using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using EducGramming.Services;
using System.Diagnostics;
using System.Linq;
using EducGramming.Views;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private UserService _userService;
        private string? _email;
        private string? _password;
        private bool _isPasswordVisible = false;
        private bool _staySignedIn;
        private bool _isBusy;
        private string _errorMessage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                    ClearErrorMessage();
                }
            }
        }

        public string? Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    ClearErrorMessage();
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool StaySignedIn
        {
            get => _staySignedIn;
            set
            {
                if (_staySignedIn != value)
                {
                    _staySignedIn = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SignInCommand { get; private set; }
        public ICommand TogglePasswordCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand ForgotPasswordCommand { get; private set; }

        public LoginViewModel()
        {
            _userService = new UserService();
            SignInCommand = new Command(async () => await OnLoginClicked());
            TogglePasswordCommand = new Command(TogglePassword);
            RegisterCommand = new Command(async () => await NavigateToRegister());
            ForgotPasswordCommand = new Command(async () => await NavigateToResetPassword());

            // Clear any existing credentials
            ClearSavedCredentials();
        }

        private async Task OnLoginClicked()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine($"Attempting to sign in with email: {Email}");

                // Clear any previous error message
                ErrorMessage = string.Empty;

                try
                {
                    // Validate login credentials
                    bool isValid = await _userService.ValidateLogin(Email, Password);
                    
                    if (isValid)
                    {
                        // Save stay signed in preference
                        Preferences.Default.Set("StaySignedIn", StaySignedIn);
                        
                        if (StaySignedIn)
                        {
                            Preferences.Default.Set("LastUsedEmail", Email);
                            Debug.WriteLine("Saved login preferences for stay signed in");
                        }
                        else
                        {
                            Preferences.Default.Remove("LastUsedEmail");
                            Debug.WriteLine("Cleared stay signed in preferences");
                        }
                        
                        // Navigate to main app
                        Debug.WriteLine("Login successful, navigating to AppShell");
                        Application.Current.MainPage = new AppShell();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Login failed: {ex.Message}");
                    ErrorMessage = ex.Message;
                    await Application.Current.MainPage.DisplayAlert("Login Failed", ErrorMessage, "OK");
                    
                    // Clear password field on error
                    Password = string.Empty;
                    return;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void TogglePassword()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ClearSavedCredentials()
        {
            try
            {
                // Clear all saved credentials
                Preferences.Default.Remove("LastUsedEmail");
                Preferences.Default.Remove("RememberEmail");
                Preferences.Default.Remove("CurrentEmail");
                Debug.WriteLine("Cleared saved credentials");
                
                // Reset view model properties
                Email = string.Empty;
                Password = string.Empty;
                StaySignedIn = false;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing credentials: {ex.Message}");
            }
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }

        private async Task NavigateToRegister()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await Shell.Current.GoToAsync("//register");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToResetPassword()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await Shell.Current.GoToAsync("//resetpassword");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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