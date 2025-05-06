using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EducGramming.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private UserService _userService;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private bool _isCurrentPasswordVisible;
        private bool _isNewPasswordVisible;
        private bool _isConfirmPasswordVisible;
        private bool _isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (_currentPassword != value)
                {
                    _currentPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCurrentPasswordVisible
        {
            get => _isCurrentPasswordVisible;
            set
            {
                if (_isCurrentPasswordVisible != value)
                {
                    _isCurrentPasswordVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNewPasswordVisible
        {
            get => _isNewPasswordVisible;
            set
            {
                if (_isNewPasswordVisible != value)
                {
                    _isNewPasswordVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set
            {
                if (_isConfirmPasswordVisible != value)
                {
                    _isConfirmPasswordVisible = value;
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

        public ICommand ChangePasswordCommand { get; private set; }
        public ICommand ToggleCurrentPasswordCommand { get; private set; }
        public ICommand ToggleNewPasswordCommand { get; private set; }
        public ICommand ToggleConfirmPasswordCommand { get; private set; }
        public ICommand BackToProfileCommand { get; private set; }

        public ChangePasswordViewModel()
        {
            _userService = new UserService();
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            ToggleCurrentPasswordCommand = new Command(() => IsCurrentPasswordVisible = !IsCurrentPasswordVisible);
            ToggleNewPasswordCommand = new Command(() => IsNewPasswordVisible = !IsNewPasswordVisible);
            ToggleConfirmPasswordCommand = new Command(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
            BackToProfileCommand = new Command(async () => await NavigateBackToProfile());
        }

        private async Task ChangePasswordAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(CurrentPassword) || 
                string.IsNullOrWhiteSpace(NewPassword) || 
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "New passwords do not match", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Get current user email
                string currentEmail = Preferences.Default.Get<string>("CurrentEmail", null);
                if (string.IsNullOrEmpty(currentEmail))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please log in again", "OK");
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // We no longer need to validate separately since it will be validated in the change password process
                // Change password - pass both old and new password
                await _userService.ChangePassword(currentEmail, CurrentPassword, NewPassword);

                await Application.Current.MainPage.DisplayAlert("Success", "Password changed successfully", "OK");
                await NavigateBackToProfile();
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

        private async Task NavigateBackToProfile()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed: " + ex.Message, "OK");
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