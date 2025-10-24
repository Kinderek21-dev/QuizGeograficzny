using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using QuizGeograficzny.Models;

namespace QuizGeograficzny.Data;

public static class QuestionsData
{
    private const string FileName = "questions.json";

    public static List<Question> GetAllQuestions()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", FileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Nie znaleziono pliku z pytaniami: {filePath}");

            string json = File.ReadAllText(filePath);

            var data = JsonSerializer.Deserialize<QuestionsWrapper>(json);

            return data?.Questions ?? new List<Question>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd przy wczytywaniu pytań: {ex.Message}");
            return new List<Question>();
        }
    }

    public static List<Question> GetByDifficulty(string difficulty)
    {
        return GetAllQuestions()
            .Where(q => string.Equals(q.Difficulty, difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private class QuestionsWrapper
    {
        public List<Question> Questions { get; set; } = new();
    }
}