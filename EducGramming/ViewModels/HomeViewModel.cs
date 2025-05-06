using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private string _welcomeMessage;
        private string _subtitleMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            private set
            {
                if (_welcomeMessage != value)
                {
                    _welcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SubtitleMessage
        {
            get => _subtitleMessage;
            private set
            {
                if (_subtitleMessage != value)
                {
                    _subtitleMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LearnCSharpCommand { get; private set; }
        public ICommand LearnJavaCommand { get; private set; }

        public HomeViewModel()
        {
            UpdateWelcomeMessage();
            LearnCSharpCommand = new Command(async () => await LearnCSharpAsync());
            LearnJavaCommand = new Command(async () => await LearnJavaAsync());
        }

        private void UpdateWelcomeMessage()
        {
            string username = Preferences.Default.Get<string>("CurrentUsername", null);
            string fullName = Preferences.Default.Get<string>("CurrentFullName", null);

            if (!string.IsNullOrEmpty(fullName))
            {
                WelcomeMessage = $"Welcome, {fullName}!";
                SubtitleMessage = "Ready to continue your programming journey?";
            }
            else if (!string.IsNullOrEmpty(username))
            {
                WelcomeMessage = $"Welcome, {username}!";
                SubtitleMessage = "Ready to continue your programming journey?";
            }
            else
            {
                WelcomeMessage = "Welcome to EducGramming!";
                SubtitleMessage = "Start your programming journey today.";
            }
        }

        private async Task LearnCSharpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//lesson?language=csharp");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task LearnJavaAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//lesson?language=java");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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