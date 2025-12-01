using QuizGeograficzny.Services;
using QuizGeograficzny.Models;
using Microsoft.Maui.Controls;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Source), "source")]
public partial class ScoreboardPage : ContentPage
{
    private string _source = "local";


    public string Source
    {
        set
        {
            _source = value;
            Title = _source == "remote" ? "Ranking Globalny" : "Tablica wyników";
            OnPropertyChanged();
        }
    }

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
        try
        {
      
            ScoresListView.ItemsSource = null;

            List<ScoreEntry> scores;

            if (_source == "remote")
            {

                scores = await RankingService.GetTopScoresAsync(50);
            }
            else
            {

                scores = await ScoreboardService.GetTopAsync(50);
            }

            ScoresListView.ItemsSource = scores;
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", "Nie uda³o siê pobraæ danych: " + ex.Message, "OK");
        }
    }

    private async void OnRefreshTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }
        await LoadScoresAsync();
    }

    private async void OnClearTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }

        if (_source == "remote")
        {
            await DisplayAlert("Info", "Ranking globalny jest wspólny dla wszystkich i nie mo¿na go wyczyœciæ.", "OK");
            return;
        }

        bool ok = await DisplayAlert("Wyczyœæ tablicê?", "Czy na pewno usun¹æ wyniki LOKALNE?", "Tak", "Nie");
        if (!ok) return;

        await ScoreboardService.ClearAsync();
        await LoadScoresAsync();
    }
}