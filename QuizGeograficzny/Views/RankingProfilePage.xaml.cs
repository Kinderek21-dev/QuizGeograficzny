using QuizGeograficzny.Services;
using QuizGeograficzny.Models;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace QuizGeograficzny.Views
{
    public partial class RankingProfilePage : ContentPage
    {
        private const string KEY_PROFILE_ID = "RankingProfileId";
        private string _profileId = string.Empty;

        public RankingProfilePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _profileId = Preferences.Get(KEY_PROFILE_ID, string.Empty);
            UpdateUIState();
            await LoadProfileStatsAsync();
        }

        private void UpdateUIState()
        {
            if (string.IsNullOrEmpty(_profileId))
            {
                ProfileActionButton.Text = "Utwórz profil";
                PlayerNameLabel.Text = "Brak profilu";
            }
            else
            {
                ProfileActionButton.Text = "Zmieñ nick";
            }
        }

        private async Task LoadProfileStatsAsync()
        {
            if (string.IsNullOrEmpty(_profileId))
            {
                GamesPlayedLabel.Text = "-";
                PercentCorrectLabel.Text = "-";
                TotalPointsLabel.Text = "-";
                CorrectTotalLabel.Text = "-";
                return;
            }

            var profile = await RankingService.GetProfileAsync(_profileId);
            if (profile != null)
            {
                PlayerNameLabel.Text = profile.PlayerName ?? "Anon";

                var stats = profile.Stats;
                if (stats != null)
                {
                    GamesPlayedLabel.Text = stats.GamesPlayed.ToString();
                    PercentCorrectLabel.Text = stats.TotalAnswers > 0 ? $"{(int)Math.Round((double)stats.CorrectAnswers / stats.TotalAnswers * 100)}%" : "0%";
                    TotalPointsLabel.Text = stats.TotalPoints.ToString();
                    CorrectTotalLabel.Text = stats.CorrectAnswers.ToString();
                }
            }
        }

        private async void OnChangeNameTapped(object sender, EventArgs e)
        {
            string title = string.IsNullOrEmpty(_profileId) ? "Utwórz profil" : "Zmieñ nick";
            string placeholder = string.IsNullOrEmpty(_profileId) ? "" : PlayerNameLabel.Text;

            var name = await DisplayPromptAsync(title, "Podaj swój nick:", "Zapisz", "Anuluj", placeholder);

            if (string.IsNullOrWhiteSpace(name)) return;
            name = name.Trim();

            if (string.IsNullOrEmpty(_profileId))
            {
                _profileId = Guid.NewGuid().ToString("N");
                Preferences.Set(KEY_PROFILE_ID, _profileId);
            }

            var profile = await RankingService.GetProfileAsync(_profileId);
            if (profile == null)
            {
                profile = new PlayerProfile
                {
                    ProfileId = _profileId,
                    PlayerName = name,
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    Stats = new QuizGeograficzny.Models.PlayerStats()
                };
            }
            else
            {
                profile.PlayerName = name;
            }

            try
            {
                await RankingService.CreateOrUpdateProfileAsync(profile);
                await DisplayAlert("Sukces", "Profil zapisany.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("B³¹d", "Nie uda³o siê zapisaæ profilu: " + ex.Message, "OK");
            }

            UpdateUIState();
            PlayerNameLabel.Text = name;
        }


        private async void OnShowGlobalRankingTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement element)
            {
                await element.ScaleTo(0.95, 100);
                await element.ScaleTo(1.0, 100);
            }

            await Shell.Current.GoToAsync("///scoreboard?source=remote");
        }
    }
}