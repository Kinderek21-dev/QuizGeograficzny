using Firebase.Database;
using Firebase.Database.Query;
using QuizGeograficzny.Models;
using System.Linq;

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
            if (string.IsNullOrWhiteSpace(profile.ProfileId)) return;

            await client.Child(ProfilesNode)
                        .Child(profile.ProfileId)
                        .PutAsync(profile);
        }

        public static async Task<PlayerProfile?> GetProfileAsync(string profileId)
        {
            try
            {
                
                var profile = await client.Child(ProfilesNode)
                                          .Child(profileId)
                                          .OnceSingleAsync<PlayerProfile>();
                return profile;
            }
            catch
            {
                return null;
            }
        }

        public static async Task UpdateStatsAfterGameAsync(string profileId, int gainedPoints, int correctAnswersInGame, int totalAnswersInGame)
        {
            var profile = await GetProfileAsync(profileId);
            if (profile == null) return;

            if (profile.Stats == null)
                profile.Stats = new QuizGeograficzny.Models.PlayerStats();

            profile.Stats.GamesPlayed++;
            profile.Stats.CorrectAnswers += correctAnswersInGame;
            profile.Stats.TotalAnswers += totalAnswersInGame;
            profile.Stats.TotalPoints += gainedPoints;

            if (gainedPoints > profile.Stats.BestScore)
                profile.Stats.BestScore = gainedPoints;

            profile.Stats.LastPlayed = DateTime.UtcNow.ToString("o");

            await CreateOrUpdateProfileAsync(profile);
        }


        public static async Task AddScoreAsync(string profileId, string playerName, ScoreEntry entry, string mode = "ranking")
        {

            var dto = new ScoreEntryDto
            {
                playerId = profileId,
                playerName = playerName,
                score = entry.Score,
                date = entry.Date.ToUniversalTime().ToString("o"),
                mode = mode,
                difficulty = entry.Difficulty
            };

            await client.Child(ScoresNode).PostAsync(dto);
        }

        public static async Task<List<ScoreEntry>> GetTopScoresAsync(int count = 50)
        {
            try
            {
              
                var collection = await client
                    .Child(ScoresNode)
                    .OrderBy("score") 
                    .LimitToLast(count)
                    .OnceAsync<ScoreEntryDto>();


                var list = collection.Select(item =>
                {
                    var dto = item.Object;
                    DateTime date = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(dto.date))
                        DateTime.TryParse(dto.date, out date);

                    return new ScoreEntry
                    {
                        PlayerName = dto.playerName ?? "Anon",
                        Score = dto.score,
                        Date = date,
                        Difficulty = dto.difficulty ?? "ranking"
                    };
                }).ToList();

 
                return list.OrderByDescending(x => x.Score).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RankingService error: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }


        public class ScoreEntryDto
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