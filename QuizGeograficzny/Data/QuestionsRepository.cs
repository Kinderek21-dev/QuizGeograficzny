using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using QuizGeograficzny.Models;

namespace QuizGeograficzny.Data
{
    public static class QuestionsRepository
    {
        private static List<Question>? _questions;

        public static IEnumerable<Question> GetAll()
        {
            if (_questions == null)
                LoadQuestions();

            return _questions!;
        }

        public static IEnumerable<Question> GetByDifficulty(string difficulty)
        {
            if (_questions == null)
                LoadQuestions();

            return _questions!
                .Where(q => q.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
        }

        private static void LoadQuestions()
        {
            try
            {
                var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");

                if (!File.Exists(jsonPath))
                    throw new FileNotFoundException($"Nie znaleziono pliku: {jsonPath}");

                var json = File.ReadAllText(jsonPath);

                var wrapper = JsonSerializer.Deserialize<QuestionWrapper>(json);

                _questions = wrapper?.Questions ?? new List<Question>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wczytywania pytań: {ex.Message}");
                _questions = new List<Question>();
            }
        }

        private class QuestionWrapper
        {
            public List<Question> Questions { get; set; } = new();
        }
    }
}