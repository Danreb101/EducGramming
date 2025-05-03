using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EducGramming.Services;

namespace EducGramming.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _username = string.Empty;
        private string _fullName = string.Empty;
        private bool _isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public string Password
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

        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }

        public RegisterViewModel()
        {
            _userService = new UserService();
            RegisterCommand = new Command(async () => await RegisterAsync());
            BackToLoginCommand = new Command(async () => await NavigateBackToLogin());
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Email) || 
                    string.IsNullOrWhiteSpace(Password) || 
                    string.IsNullOrWhiteSpace(Username) || 
                    string.IsNullOrWhiteSpace(FullName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                    return;
                }

                // Register user using UserService
                await _userService.RegisterUser(Email, Password, Username, FullName);

                await Application.Current.MainPage.DisplayAlert("Success", "Registration successful! Please login with your credentials.", "OK");
                await NavigateBackToLogin();
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

        private async Task NavigateBackToLogin()
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 