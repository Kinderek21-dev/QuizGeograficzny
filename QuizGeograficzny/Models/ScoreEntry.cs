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
    public class ScoreForFirebase : ScoreEntry
    {
        public string PlayerId { get; set; } = string.Empty;
        public string Mode { get; set; } = "ranking";
    }
}
