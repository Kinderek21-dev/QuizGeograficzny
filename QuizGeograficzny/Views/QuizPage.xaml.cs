using QuizGeograficzny.Models;
using QuizGeograficzny.Data;

namespace QuizGeograficzny.Views;

public partial class QuizPage : ContentPage
{
    private List<Question> _questions;
    private int _currentIndex = 0;
    private int _score = 0;

    public QuizPage()
    {
        InitializeComponent();
        _questions = QuestionsData.GetSampleQuestions();
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        if (_currentIndex >= _questions.Count)
        {
            DisplayAlert("Koniec quizu", $"Twój wynik: {_score} punktów", "OK");
            return;
        }

        var q = _questions[_currentIndex];
        QuestionText.Text = q.QuestionText;

        var buttons = QuizLayout.Children.OfType<Button>().ToList();


        if (buttons != null && q.Answers.Length == buttons.Count)
        {
            for (int i = 0; i < buttons.Count; i++)
                buttons[i].Text = q.Answers[i];
        }
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
 
        if (_currentIndex >= _questions.Count)
        {
            
            await DisplayAlert("Koniec!", "", "Zobacz wynik");
            await Shell.Current.GoToAsync("///result");
            return;
        }

        var q = _questions[_currentIndex];


        int chosenIndex = Array.IndexOf(q.Answers, button?.Text);

        if (chosenIndex == q.CorrectAnswerIndex)
        {
            _score++;
            await DisplayAlert("Poprawna odpowiedŸ!", "Dobrze!", "OK");
        }
        else
        {
            await DisplayAlert("Z³a odpowiedŸ!", "Spróbuj nastêpnym razem.", "OK");
        }

        _currentIndex++;
        LoadQuestion();
    }
}
