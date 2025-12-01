using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGeograficzny.Models
{
    public class PlayerProfile
    {
        public string ProfileId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = "Anon";
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public PlayerStats Stats { get; set; } = new();
    }

    public class PlayerStats
    {
        public int GamesPlayed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
        public int TotalPoints { get; set; }
        public int BestScore { get; set; }
        public string LastPlayed { get; set; } = string.Empty;
    }
}