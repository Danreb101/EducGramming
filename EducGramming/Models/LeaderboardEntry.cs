using System;

namespace EducGramming.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsCurrentUser { get; set; }
    }
} 