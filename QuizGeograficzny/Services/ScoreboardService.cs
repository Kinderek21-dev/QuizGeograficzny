using QuizGeograficzny.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;

namespace QuizGeograficzny.Services
{
    public static class ScoreboardService
    {
        private const string FirebaseUrl = "https://quizgeograficzny-default-rtdb.europe-west1.firebasedatabase.app/";
        private const string CollectionName = "scores";

        private static readonly FirebaseClient firebaseClient = new FirebaseClient(FirebaseUrl);

        public static async Task<List<ScoreEntry>> GetAllAsync()
        {
            try
            {
                var scores = await firebaseClient
                    .Child(CollectionName)
                    .OnceAsync<ScoreEntry>();

                return scores.Select(item => new ScoreEntry
                {
                    PlayerName = item.Object.PlayerName,
                    Score = item.Object.Score,
                    Date = item.Object.Date,
                    Difficulty = item.Object.Difficulty
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd pobierania z Firebase: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }

        public static async Task AddAsync(ScoreEntry entry)
        {
            try
            {
                await firebaseClient
                    .Child(CollectionName)
                    .PostAsync(entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu do Firebase: {ex.Message}");
            }
        }

        public static async Task<List<ScoreEntry>> GetTopAsync(int count = 50)
        {
            var list = await GetAllAsync();
            return list.OrderByDescending(x => x.Score).Take(count).ToList();
        }

        public static async Task ClearAsync()
        {
            try
            {
                await firebaseClient.Child(CollectionName).DeleteAsync();
            }
            catch { /* ignore */ }
        }
    }
}