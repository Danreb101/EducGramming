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
            _userService = new UserService();
            RegisterCommand = new Command(async () => await RegisterAsync());
            BackToLoginCommand = new Command(async () => await NavigateToLogin());
            TogglePasswordCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ShowTermsCommand = new Command(() => ShowTermsRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password) || 
                string.IsNullOrWhiteSpace(FullName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (!IsTermsAccepted)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please accept the terms and conditions", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Register user with email, password, and full name only
                await _userService.RegisterUser(Email, Password, FullName);

                await Application.Current.MainPage.DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                await NavigateToLogin();
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

        private async Task NavigateToLogin()
        {
            try
            {
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
            }
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

                // Register user with Firebase through UserService
                await _userService.RegisterUser(Email, Password, FullName);
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