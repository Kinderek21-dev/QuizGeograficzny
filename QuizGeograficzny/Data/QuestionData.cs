using System.Text.Json;
using System.Text.Json.Serialization;
using QuizGeograficzny.Models;
using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Data;

public static class QuestionsData
{
    private const string FileName = "questions.json";

    private static List<Question>? _cached;

    public static async Task<List<Question>> GetAllQuestionsAsync()
    {
        if (_cached != null)
            return _cached;

        try
        {
  
            using var stream = await FileSystem.OpenAppPackageFileAsync(FileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var wrapper = JsonSerializer.Deserialize<QuestionsWrapper>(json, opts);
            _cached = wrapper?.Questions ?? new List<Question>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd przy wczytywaniu pytań: {ex.Message}");
            _cached = new List<Question>();
        }

        return _cached;
    }

    public static async Task<List<Question>> GetByDifficultyAsync(string difficulty)
    {
        var all = await GetAllQuestionsAsync();
        return all
            .Where(q => string.Equals(q.Difficulty, difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private class QuestionsWrapper
    {
        [JsonPropertyName("questions")]
        public List<Question> Questions { get; set; } = new();
    }
}
