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
                ScoreValueLabel.Text = _score.ToString();
            }
        }
    }

    private async void OnReplayTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }
        await Shell.Current.GoToAsync("///gamemode");
    }

    private async void OnScoreboardTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }
        await Shell.Current.GoToAsync("///scoreboard");
    }
}