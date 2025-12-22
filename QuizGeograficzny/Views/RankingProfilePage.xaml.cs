using QuizGeograficzny.Models;
using QuizGeograficzny.Services;

namespace QuizGeograficzny.Views;

public partial class RankingProfilePage : ContentPage
{
    public RankingProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfile();
    }

    private async Task LoadProfile()
    {
      
        string savedId = Preferences.Get("LocalProfileId", string.Empty);

        if (!string.IsNullOrEmpty(savedId))
        {
            var profile = await RankingService.GetProfileAsync(savedId);
            if (profile != null)
            {

                PlayerNameLabel.Text = profile.PlayerName;

                if (profile.Stats != null)
                {
                    GamesPlayedLabel.Text = profile.Stats.GamesPlayed.ToString();

                    double percent = profile.Stats.TotalAnswers > 0
                        ? (double)profile.Stats.CorrectAnswers / profile.Stats.TotalAnswers
                        : 0;
                    PercentCorrectLabel.Text = $"{percent:P0}"; 

                    TotalPointsLabel.Text = profile.Stats.TotalPoints.ToString();
                    CorrectTotalLabel.Text = profile.Stats.CorrectAnswers.ToString();
                }
            }
        }
    }

 
    private async void OnChangeNameTapped(object sender, EventArgs e)
    {
      
        string newName = await DisplayPromptAsync("Zmiana Nicku", "Podaj swój nowy nick:", "Zapisz", "Anuluj", placeholder: PlayerNameLabel.Text);

        if (string.IsNullOrWhiteSpace(newName))
            return;

        string currentId = Preferences.Get("LocalProfileId", string.Empty);

  
        var profile = new PlayerProfile
        {
            ProfileId = string.IsNullOrEmpty(currentId) ? Guid.NewGuid().ToString() : currentId,
            PlayerName = newName
        };

        if (!string.IsNullOrEmpty(currentId))
        {
            var existing = await RankingService.GetProfileAsync(currentId);
            if (existing != null)
            {
                profile.Stats = existing.Stats;
                profile.CreatedAt = existing.CreatedAt;
            }
        }

     
        await RankingService.CreateOrUpdateProfileAsync(profile);

     
        Preferences.Set("LocalProfileId", profile.ProfileId);

    
        PlayerNameLabel.Text = newName;
        await DisplayAlert("Sukces", "Nick zosta³ zaktualizowany.", "OK");
    }

    
    private async void OnShowGlobalRankingTapped(object sender, TappedEventArgs e)
    {
        
        await Shell.Current.GoToAsync("//ranking");
    }
}