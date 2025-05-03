using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EducGramming.ViewModels
{
    public class ResetPasswordViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;
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

        public ICommand BackToLoginCommand { get; private set; }

        public ResetPasswordViewModel()
        {
            BackToLoginCommand = new Command(async () => await NavigateBackToLogin());
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