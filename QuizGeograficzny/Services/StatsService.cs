using Microsoft.Maui.Storage;
using QuizGeograficzny.Models;
using System.Text.Json;

namespace QuizGeograficzny.Services
{
    public class PlayerStats
    {
        public string PlayerName { get; set; } = "Anon";
        public int GamesPlayed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
    }

    public static class StatsService
    {
        private const string KEY = "Quiz_PlayerStats_v1";

        public static PlayerStats GetStats()
        {
            try
            {
                var json = Preferences.Get(KEY, null);
                if (string.IsNullOrEmpty(json)) return new PlayerStats();
                return JsonSerializer.Deserialize<PlayerStats>(json) ?? new PlayerStats();
            }
            catch { return new PlayerStats(); }
        }

        public static void SaveStats(PlayerStats s)
        {
            try
            {
                var json = JsonSerializer.Serialize(s);
                Preferences.Set(KEY, json);
            }
            catch { }
        }

        public static async Task RecordGameAsync(int score, int correctAnswers, int totalQuestions)
        {
            var s = GetStats();
            s.GamesPlayed++;
            s.CorrectAnswers += correctAnswers;
            s.TotalQuestions += totalQuestions;
            s.TotalPoints += score;
            SaveStats(s);

            await Task.CompletedTask;
        }
    }
}
