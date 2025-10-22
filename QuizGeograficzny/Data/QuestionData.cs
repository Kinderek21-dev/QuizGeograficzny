using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuizGeograficzny.Models;

namespace QuizGeograficzny.Data;

public static class QuestionsData
{
    public static List<Question> GetSampleQuestions()
    {
        return new List<Question>
        {
            new Question
            {
                QuestionText = "Stolicą Francji jest:",
                Answers = new[] { "Paryż", "Berlin", "Madryt", "Rzym" },
                CorrectAnswerIndex = 0,
                Difficulty = "łatwy"
            },
            new Question
            {
                QuestionText = "Najdłuższa rzeka w Polsce to:",
                Answers = new[] { "Wisła", "Odra", "Warta", "San" },
                CorrectAnswerIndex = 0,
                Difficulty = "łatwy"
            },
            new Question
            {
                QuestionText = "Który kontynent ma najwięcej państw?",
                Answers = new[] { "Azja", "Afryka", "Europa", "Ameryka Południowa" },
                CorrectAnswerIndex = 1,
                Difficulty = "średni"
            }
        };
    }
}
