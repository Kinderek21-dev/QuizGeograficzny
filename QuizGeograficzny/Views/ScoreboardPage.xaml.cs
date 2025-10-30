using QuizGeograficzny.Services;
using QuizGeograficzny.Models;

namespace QuizGeograficzny.Views;

public partial class ScoreboardPage : ContentPage
{
    public ScoreboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadScoresAsync();
    }

    private async Task LoadScoresAsync()
    {
        var top = await ScoreboardService.GetTopAsync(50);
        ScoresListView.ItemsSource = top;
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadScoresAsync();
    }

    private async void OnClearClicked(object sender, EventArgs e)
    {
        bool ok = await DisplayAlert("Wyczyœæ tablicê?", "Czy na pewno chcesz usun¹æ wszystkie wyniki?", "Tak", "Nie");
        if (!ok) return;
        await ScoreboardService.ClearAsync();
        await LoadScoresAsync();
    }
}
