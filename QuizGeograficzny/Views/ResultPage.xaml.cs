using Microsoft.Maui.Controls;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Score), "score")]
public partial class ResultPage : ContentPage
{
    private int _score = 0;
    public ResultPage()
    {
        InitializeComponent();
    }

    public string Score
    {
        set
        {
            if (int.TryParse(value, out var s))
            {
                _score = s;
                ResultLabel.Text = $"Twój wynik: {_score} punktów";
            }
        }
    }

    private async void OnReplayClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///difficulty");
    }

    private async void OnShowScoreboardClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///scoreboard");
    }
}
