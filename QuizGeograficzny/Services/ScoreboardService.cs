using QuizGeograficzny.Models;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Services
{
    public static class ScoreboardService
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "scores.json"
        );


        public static async Task<List<ScoreEntry>> GetAllAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new List<ScoreEntry>();

                using var stream = File.OpenRead(FilePath);
                var list = await JsonSerializer.DeserializeAsync<List<ScoreEntry>>(stream);
                return list ?? new List<ScoreEntry>();
            }
            catch
            {
                return new List<ScoreEntry>();
            }
        }

        public static async Task AddAsync(ScoreEntry entry)
        {
            var list = await GetAllAsync();
            list.Add(entry);
            using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, list, new JsonSerializerOptions { WriteIndented = true });
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
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch { /* ignore */ }
        }
    }
}
