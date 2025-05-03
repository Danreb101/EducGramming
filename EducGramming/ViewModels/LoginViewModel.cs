using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using EducGramming.Services;

namespace EducGramming.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private string? _email;
        private string? _password;
        private bool _isPasswordVisible = false;
        private bool _isTermsAccepted;
        private bool _staySignedIn;
        private bool _isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ShowTermsRequested;

        public string? Email
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

        public string? Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
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

        public bool IsTermsAccepted
        {
            get => _isTermsAccepted;
            set
            {
                if (_isTermsAccepted != value)
                {
                    _isTermsAccepted = value;
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

        public ICommand SignInCommand { get; private set; }
        public ICommand TogglePasswordCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand ForgotPasswordCommand { get; private set; }
        public ICommand ShowTermsCommand { get; private set; }

        public LoginViewModel()
        {
            _userService = new UserService();
            SignInCommand = new Command(async () => await SignInAsync());
            TogglePasswordCommand = new Command(TogglePassword);
            RegisterCommand = new Command(async () => await NavigateToRegister());
            ForgotPasswordCommand = new Command(async () => await NavigateToResetPassword());
            ShowTermsCommand = new Command(ShowTerms);

            // Clear any existing credentials
            ClearSavedCredentials();
        }

        private void ShowTerms()
        {
            ShowTermsRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task SignInAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both email and password", "OK");
                return;
            }

            if (!IsTermsAccepted)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please accept the Terms and Conditions", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Validate login credentials
                bool isValid = await _userService.ValidateLogin(Email, Password);
                
                if (!isValid)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid email or password", "OK");
                    Password = string.Empty; // Clear password for security
                    return;
                }

                // Save whether to remember email
                if (StaySignedIn)
                {
                    Preferences.Default.Set("LastUsedEmail", Email);
                    Preferences.Default.Set("RememberEmail", true);
                }
                else
                {
                    Preferences.Default.Remove("LastUsedEmail");
                    Preferences.Default.Set("RememberEmail", false);
                }
                
                // Navigate to main app by switching the entire shell
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("///home");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Login failed: " + ex.Message, "OK");
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
            // Clear all saved credentials
            Preferences.Default.Remove("LastUsedEmail");
            Preferences.Default.Remove("RememberEmail");
            Preferences.Default.Remove("CurrentEmail");
            
            // Reset view model properties
            Email = string.Empty;
            Password = string.Empty;
            StaySignedIn = false;
            IsTermsAccepted = false;
        }

        private async Task NavigateToRegister()
        {
            try
            {
                await Shell.Current.GoToAsync("//register");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
            }
        }

        private async Task NavigateToResetPassword()
        {
            try
            {
                await Shell.Current.GoToAsync("//resetpassword");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 