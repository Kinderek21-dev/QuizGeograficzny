using System;

namespace QuizGeograficzny.Models
{
    public class ScoreEntry
    {
        public string PlayerName { get; set; } = "Anon";
        public int Score { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Difficulty { get; set; } = string.Empty;
    }
}
