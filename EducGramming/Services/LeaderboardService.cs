using EducGramming.Models;
using System.Collections.ObjectModel;

namespace EducGramming.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private const string LeaderboardKey = "Leaderboard";
        private ObservableCollection<LeaderboardEntry> _leaderboard;

        public LeaderboardService()
        {
            _leaderboard = LoadLeaderboard();
        }

        private ObservableCollection<LeaderboardEntry> LoadLeaderboard()
        {
            var jsonString = Preferences.Default.Get(LeaderboardKey, string.Empty);
            if (string.IsNullOrEmpty(jsonString))
            {
                return new ObservableCollection<LeaderboardEntry>();
            }

            try
            {
                var entries = System.Text.Json.JsonSerializer.Deserialize<List<LeaderboardEntry>>(jsonString);
                return new ObservableCollection<LeaderboardEntry>(entries ?? new List<LeaderboardEntry>());
            }
            catch
            {
                return new ObservableCollection<LeaderboardEntry>();
            }
        }

        private void SaveLeaderboard()
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(_leaderboard.ToList());
            Preferences.Default.Set(LeaderboardKey, jsonString);
        }

        private void UpdateRanks()
        {
            var sortedEntries = _leaderboard.OrderByDescending(e => e.Score).ToList();
            for (int i = 0; i < sortedEntries.Count; i++)
            {
                sortedEntries[i].Rank = i + 1;
            }
            _leaderboard = new ObservableCollection<LeaderboardEntry>(sortedEntries);
        }

        public async Task<ObservableCollection<LeaderboardEntry>> GetLeaderboardAsync()
        {
            var currentName = Preferences.Default.Get("CurrentName", "Player");
            foreach (var entry in _leaderboard)
            {
                entry.IsCurrentUser = entry.Name == currentName;
            }
            return _leaderboard;
        }

        public async Task UpdateScoreAsync(string name, int score)
        {
            var entry = _leaderboard.FirstOrDefault(e => e.Name == name);
            if (entry == null)
            {
                entry = new LeaderboardEntry
                {
                    Name = name,
                    Score = score,
                    LastUpdated = DateTime.Now
                };
                _leaderboard.Add(entry);
            }
            else if (score > entry.Score)
            {
                entry.Score = score;
                entry.LastUpdated = DateTime.Now;
            }

            UpdateRanks();
            SaveLeaderboard();
        }

        public async Task<int> GetUserRankAsync(string name)
        {
            var entry = _leaderboard.FirstOrDefault(e => e.Name == name);
            return entry?.Rank ?? 0;
        }

        public async Task<int> GetUserScoreAsync(string name)
        {
            var entry = _leaderboard.FirstOrDefault(e => e.Name == name);
            return entry?.Score ?? 0;
        }
    }
} 