using System;
using Microsoft.Maui.ApplicationModel;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Score), "score")]
[QueryProperty(nameof(Mode), "mode")] 
public partial class ResultPage : ContentPage
{
    private int _score = 0;
    private string _mode = string.Empty;

    public ResultPage()
    {
        InitializeComponent();
    }

    public string Mode
    {
        set
        {
            _mode = value;
        }
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


        if (string.Equals(_mode, "ranking", StringComparison.OrdinalIgnoreCase))
        {
            await Shell.Current.GoToAsync("///rankingprofile");
        }
        else
        {
            await Shell.Current.GoToAsync("///scoreboard");
        }
    }
}