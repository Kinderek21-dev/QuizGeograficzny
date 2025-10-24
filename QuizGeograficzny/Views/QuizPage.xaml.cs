using QuizGeograficzny.Data;
using QuizGeograficzny.Models;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Difficulty), "difficulty")]
public partial class QuizPage : ContentPage
{
    private List<Question> _questions = new();
    private int _currentIndex = 0;
    private int _score = 0;
    private string _difficulty = "≥atwy";

    public QuizPage()
    {
        InitializeComponent();
        // jeúli Difficulty zostanie ustawione przez QueryProperty, setter wywo≥a LoadAndShow()
        // jeúli nie ó moøesz rÍcznie ustawiÊ Difficulty przed pokazaniem strony
    }

    // property przypisywane przez Shell (///quiz?difficulty=≥atwy)
    public string Difficulty
    {
        get => _difficulty;
        set
        {
            _difficulty = value ?? "≥atwy";
            // ≥adujemy pytania i pokazujemy pierwsze
            LoadQuestions();
            DisplayQuestion();
        }
    }

    private void LoadQuestions()
    {
        // uøywamy QuestionsData.GetByDifficulty(...) ó upewnij siÍ, øe ta metoda istnieje
        _questions = QuestionsData.GetByDifficulty(_difficulty).ToList();

        // fallback: jeøeli brak pytaÒ dla poziomu, pobierz wszystkie
        if (_questions == null || _questions.Count == 0)
        {
            _questions = QuestionsData.GetAllQuestions();
        }

        _currentIndex = 0;
        _score = 0;
    }

    private void DisplayQuestion()
    {
        if (_questions == null || _questions.Count == 0)
        {
            QuestionLabel.Text = "Brak pytaÒ.";
            ProgressLabel.Text = "";
            AnswerButton1.IsVisible = AnswerButton2.IsVisible = AnswerButton3.IsVisible = AnswerButton4.IsVisible = false;
            return;
        }

        if (_currentIndex >= _questions.Count)
        {
            EndQuiz();
            return;
        }

        var q = _questions[_currentIndex];

        ProgressLabel.Text = $"Pytanie {_currentIndex + 1} / {_questions.Count}";
        QuestionLabel.Text = q.QuestionText;

        // ustaw odpowiedzi (przyjmujemy, øe zawsze 4)
        AnswerButton1.Text = q.Answers.Length > 0 ? q.Answers[0] : "";
        AnswerButton2.Text = q.Answers.Length > 1 ? q.Answers[1] : "";
        AnswerButton3.Text = q.Answers.Length > 2 ? q.Answers[2] : "";
        AnswerButton4.Text = q.Answers.Length > 3 ? q.Answers[3] : "";

        // obrazek
        if (!string.IsNullOrEmpty(q.ImageUrl))
        {
            QuestionImage.IsVisible = true;
            QuestionImage.Source = q.ImageUrl;
        }
        else
        {
            QuestionImage.IsVisible = false;
            QuestionImage.Source = null;
        }
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (_questions == null || _questions.Count == 0) return;
        if (_currentIndex >= _questions.Count)
        {
            await EndQuiz();
            return;
        }

        var q = _questions[_currentIndex];

        int chosenIndex = -1;
        if (btn == AnswerButton1) chosenIndex = 0;
        else if (btn == AnswerButton2) chosenIndex = 1;
        else if (btn == AnswerButton3) chosenIndex = 2;
        else if (btn == AnswerButton4) chosenIndex = 3;

        // punktacja wg poziomu
        int plus = _difficulty.ToLower() switch
        {
            "≥atwy" => 1,
            "úredni" => 2,
            "trudny" => 3,
            "bardzo trudny" => 5,
            _ => 1
        };
        int minus = _difficulty.ToLower() switch
        {
            "≥atwy" => 2,
            _ => 0
        };

        if (chosenIndex == q.CorrectAnswerIndex)
        {
            _score += plus;
            await DisplayAlert("Poprawnie", $"+{plus} pkt", "OK");
        }
        else
        {
            _score -= minus;
            if (minus > 0)
                await DisplayAlert("èle", $"-{minus} pkt", "OK");
            else
                await DisplayAlert("èle", "0 pkt", "OK");
        }

        _currentIndex++;
        DisplayQuestion();
    }

    private async Task EndQuiz()
    {
        if (_score < 0) _score = 0;

        await Shell.Current.GoToAsync($"///result?score={_score}", animate: true);
    }
}
