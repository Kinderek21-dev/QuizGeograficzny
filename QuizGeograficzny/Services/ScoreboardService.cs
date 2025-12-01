using QuizGeograficzny.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace QuizGeograficzny.Services
{
   
    public static class ScoreboardService
    {
        private const string KEY_LOCAL_SCORES = "Local_Scores_List_v2";

        public static async Task<List<ScoreEntry>> GetAllAsync()
        {

            string json = Preferences.Get(KEY_LOCAL_SCORES, "[]");
            var list = JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();

            
            return await Task.FromResult(list);
        }

        public static async Task AddAsync(ScoreEntry entry)
        {
            var list = await GetAllAsync();
            list.Add(entry);


            var sorted = list.OrderByDescending(x => x.Score).Take(50).ToList();
            string json = JsonSerializer.Serialize(sorted);
            Preferences.Set(KEY_LOCAL_SCORES, json);
        }

        public static async Task<List<ScoreEntry>> GetTopAsync(int count = 50)
        {
            var list = await GetAllAsync();
            return list.OrderByDescending(x => x.Score).Take(count).ToList();
        }

        public static async Task ClearAsync()
        {
            Preferences.Remove(KEY_LOCAL_SCORES);
            await Task.CompletedTask;
        }
    }
}