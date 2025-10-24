using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGeograficzny.Models;

public class Question
{
    public string QuestionText { get; set; } = string.Empty;
    public string[] Answers { get; set; } = Array.Empty<string>();
    public int CorrectAnswerIndex { get; set; }   
    public string Difficulty { get; set; } = "łatwy"; 
}
