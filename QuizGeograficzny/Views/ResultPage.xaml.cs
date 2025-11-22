using System;
using Microsoft.Maui.ApplicationModel;

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


                double fraction = Math.Clamp(_score / 10.0, 0.0, 1.0);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        ScorePercentLabel.Text = $"{(int)Math.Round(fraction * 100)}%";
                        await ScoreProgressBar.ProgressTo(fraction, 600u, Easing.CubicInOut);
                    }
                    catch
                    {

                        ScoreProgressBar.Progress = fraction;
                        ScorePercentLabel.Text = $"{(int)Math.Round(fraction * 100)}%";
                    }
                });
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