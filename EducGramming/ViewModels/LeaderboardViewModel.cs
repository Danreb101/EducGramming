using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EducGramming.Models;
using EducGramming.Services;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EducGramming.ViewModels
{
    public class LeaderboardViewModel : INotifyPropertyChanged
    {
        private ILeaderboardService _leaderboardService;
        private ObservableCollection<LeaderboardEntry> _entries = new();
        private bool _isRefreshing;
        private int _currentUserRank;
        private int _currentUserScore;
        private string _currentUsername = "Player";

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LeaderboardEntry> Entries
        {
            get => _entries;
            set
            {
                if (_entries != value)
                {
                    _entries = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentUserRank
        {
            get => _currentUserRank;
            set
            {
                if (_currentUserRank != value)
                {
                    _currentUserRank = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentUserScore
        {
            get => _currentUserScore;
            set
            {
                if (_currentUserScore != value)
                {
                    _currentUserScore = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentUsername
        {
            get => _currentUsername;
            set
            {
                if (_currentUsername != value)
                {
                    _currentUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        public Command RefreshCommand { get; }

        public LeaderboardViewModel(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
            RefreshCommand = new Command(async () => await LoadLeaderboardAsync());
            LoadLeaderboardAsync().ConfigureAwait(false);
        }

        public async Task LoadLeaderboardAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                Entries = await _leaderboardService.GetLeaderboardAsync();
                CurrentUserRank = await _leaderboardService.GetUserRankAsync(CurrentUsername);
                CurrentUserScore = await _leaderboardService.GetUserScoreAsync(CurrentUsername);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load leaderboard", "OK");
            }
            finally
            {
                IsRefreshing = false;
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