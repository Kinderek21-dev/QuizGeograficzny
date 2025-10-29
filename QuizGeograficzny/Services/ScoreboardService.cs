using System.Text.Json;
using QuizGeograficzny.Models;
using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Services
{
    public static class ScoreboardService
    {
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "scoreboard.json");

        public static async Task<List<ScoreEntry>> GetAllAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new List<ScoreEntry>();

                var json = await File.ReadAllTextAsync(FilePath);
                var list = JsonSerializer.Deserialize<List<ScoreEntry>>(json);
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
            list = list.OrderByDescending(x => x.Score).ThenBy(x => x.Date).ToList();
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        public static async Task<List<ScoreEntry>> GetTopAsync(int n = 10)
        {
            var all = await GetAllAsync();
            return all.OrderByDescending(x => x.Score).Take(n).ToList();
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
