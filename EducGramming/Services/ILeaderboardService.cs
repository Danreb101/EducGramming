using EducGramming.Models;
using System.Collections.ObjectModel;

namespace EducGramming.Services
{
    public interface ILeaderboardService
    {
        Task<ObservableCollection<LeaderboardEntry>> GetLeaderboardAsync();
        Task UpdateScoreAsync(string username, int score);
        Task<int> GetUserRankAsync(string username);
        Task<int> GetUserScoreAsync(string username);
    }
} 