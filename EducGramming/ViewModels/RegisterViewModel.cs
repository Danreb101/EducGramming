using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EducGramming.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace EducGramming.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private UserService _userService;
        public event EventHandler ShowTermsRequested;
        public event EventHandler<bool> TermsAccepted;

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPasswordVisible;

        [ObservableProperty]
        private bool isTermsAccepted;

        [ObservableProperty]
        private bool isBusy;

        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }
        public ICommand TogglePasswordCommand { get; private set; }
        public ICommand ShowTermsCommand { get; private set; }

        public RegisterViewModel()
        {
            try
            {
                _userService = new UserService();
                RegisterCommand = new AsyncRelayCommand(RegisterAsync);
                BackToLoginCommand = new AsyncRelayCommand(BackToLoginAsync);
                TogglePasswordCommand = new RelayCommand(TogglePassword);
                ShowTermsCommand = new RelayCommand(ShowTerms);
                
                // Initialize defaults
                this.Email = string.Empty;
                this.Password = string.Empty;
                this.FullName = string.Empty;
                
                Debug.WriteLine("RegisterViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing RegisterViewModel: {ex.Message}");
            }
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Email) || 
                    string.IsNullOrWhiteSpace(Password) || 
                    string.IsNullOrWhiteSpace(FullName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                    return;
                }

                if (!IsTermsAccepted)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please accept the Terms and Conditions", "OK");
                    return;
                }

                // Use email as username
                string username = Email.Split('@')[0];

                // Register user with Firebase through UserService
                await _userService.RegisterUser(Email, Password, username, FullName);

                await Application.Current.MainPage.DisplayAlert("Success", "Registration successful! Please login with your credentials.", "OK");
                await BackToLoginAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackToLoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
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

        private void ShowTerms()
        {
            ShowTermsRequested?.Invoke(this, EventArgs.Empty);
        }

        public void HandleTermsAcceptance(bool accepted)
        {
            IsTermsAccepted = accepted;
            TermsAccepted?.Invoke(this, accepted);
        }

        public async Task<bool> RegisterUserAsync()
        {
            if (IsBusy) return false;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Email) || 
                    string.IsNullOrWhiteSpace(Password) || 
                    string.IsNullOrWhiteSpace(FullName))
                {
                    return false;
                }

                if (!IsTermsAccepted)
                {
                    return false;
                }

                // Use email as username
                string username = Email.Split('@')[0];

                // Register user with Firebase through UserService
                await _userService.RegisterUser(Email, Password, username, FullName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Registration Error", ex.Message, "OK");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 