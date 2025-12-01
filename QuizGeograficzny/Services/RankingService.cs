using Firebase.Database;
using Firebase.Database.Query;
using QuizGeograficzny.Models;

namespace QuizGeograficzny.Services
{
    public static class RankingService
    {
        private const string FirebaseUrl = "https://quizgeograficzny-default-rtdb.europe-west1.firebasedatabase.app/";
        private const string ProfilesNode = "profiles";
        private const string ScoresNode = "scores";
        private static readonly FirebaseClient client = new FirebaseClient(FirebaseUrl);
        public static async Task CreateOrUpdateProfileAsync(PlayerProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.ProfileId))
                throw new ArgumentException("ProfileId required");

            await client.Child(ProfilesNode)
                        .Child(profile.ProfileId)
                        .PutAsync(profile);
        }

        public static async Task<PlayerProfile?> GetProfileAsync(string profileId)
        {
            try
            {
                var item = await client.Child(ProfilesNode).Child(profileId).OnceSingleAsync<PlayerProfile>();
                return item;
            }
            catch
            {
                return null;
            }
        }

        public static async Task AddScoreAsync(string profileId, string playerName, ScoreEntry entry, string mode = "ranking")
        {
            var obj = new
            {
                playerId = profileId,
                playerName = playerName,
                score = entry.Score,
                date = entry.Date.ToUniversalTime().ToString("o"),
                mode = mode,
                difficulty = entry.Difficulty
            };

            await client.Child(ScoresNode).PostAsync(obj);
        }

        public static async Task UpdateStatsAfterGameAsync(string profileId, int gainedPoints, int correctAnswersInGame, int totalAnswersInGame)
        {
            var profile = await GetProfileAsync(profileId);
            if (profile == null) return;

            var s = profile.Stats;

      
            if (s == null) s = new QuizGeograficzny.Models.PlayerStats();

            s.GamesPlayed++;
            s.CorrectAnswers += correctAnswersInGame;
            s.TotalAnswers += totalAnswersInGame;
            s.TotalPoints += gainedPoints;
            if (gainedPoints > s.BestScore) s.BestScore = gainedPoints;
            s.LastPlayed = DateTime.UtcNow.ToString("o");

            profile.Stats = s;
            await CreateOrUpdateProfileAsync(profile);
        }
        public static async Task<List<ScoreEntry>> GetTopScoresAsync(int count = 50)
        {
            try
            {
                var scores = await client.Child(ScoresNode).OnceAsync<object>();
                var list = new List<ScoreEntry>();
                foreach (var s in scores)
                {
                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(s.Object);
                        var dto = System.Text.Json.JsonSerializer.Deserialize<ScoreEntryDto>(json);
                        if (dto != null)
                        {
                            DateTime date = DateTime.UtcNow;
                            if (!string.IsNullOrEmpty(dto.date))
                                DateTime.TryParse(dto.date, out date);

                            list.Add(new ScoreEntry
                            {
                                PlayerName = dto.playerName ?? "Anon",
                                Score = dto.score,
                                Date = date,
                                Difficulty = dto.difficulty ?? string.Empty
                            });
                        }
                    }
                    catch { }
                }
                return list.OrderByDescending(x => x.Score).Take(count).ToList();
            }
            catch
            {
                return new List<ScoreEntry>();
            }
        }

        private class ScoreEntryDto
        {
            public string? playerId { get; set; }
            public string? playerName { get; set; }
            public int score { get; set; }
            public string? date { get; set; }
            public string? difficulty { get; set; }
            public string? mode { get; set; }
        }
    }
}
