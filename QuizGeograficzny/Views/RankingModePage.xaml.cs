using QuizGeograficzny.Services;
using QuizGeograficzny.Models;
using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Views;

public partial class RankingProfilePage : ContentPage
{
    private const string KEY_PLAYER_NAME = "Ranking_PlayerName";

    public RankingProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadLocalStats();
        await LoadGlobalRankingAsync();
    }

    private void LoadLocalStats()
    {
        var stats = StatsService.GetStats();
        PlayerNameLabel.Text = stats.PlayerName ?? "Anon";
        GamesPlayedLabel.Text = stats.GamesPlayed.ToString();
        PercentCorrectLabel.Text = stats.TotalQuestions > 0 ? $"{(int)Math.Round((double)stats.CorrectAnswers / stats.TotalQuestions * 100)}%" : "0%";
        TotalPointsLabel.Text = stats.TotalPoints.ToString();
        CorrectTotalLabel.Text = stats.CorrectAnswers.ToString();
    }

    private async Task LoadGlobalRankingAsync()
    {

        var top = await ScoreboardService.GetTopAsync(50);
        GlobalRankingList.ItemsSource = top;
    }

    private async void OnChangeNameTapped(object sender, EventArgs e)
    {
        string current = PlayerNameLabel.Text ?? "Anon";
        var name = await DisplayPromptAsync("Zmieñ nick", "Podaj nick:", "OK", "Anuluj", current, -1, Keyboard.Text);
        if (!string.IsNullOrWhiteSpace(name))
        {
            var stats = StatsService.GetStats();
            stats.PlayerName = name;
            StatsService.SaveStats(stats);
            PlayerNameLabel.Text = name;
            Preferences.Set(KEY_PLAYER_NAME, name);
            await DisplayAlert("Zapisano", "Nick zosta³ zaktualizowany.", "OK");
        }
    }

    private async void OnRefreshGlobalTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement el)
        {
            await el.ScaleTo(0.95, 80);
            await el.ScaleTo(1.0, 80);
        }
        await LoadGlobalRankingAsync();
    }
}
