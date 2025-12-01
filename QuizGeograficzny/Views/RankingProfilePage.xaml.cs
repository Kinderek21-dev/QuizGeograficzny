using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using QuizGeograficzny.Models;
using QuizGeograficzny.Services;

namespace QuizGeograficzny.Views
{
    public partial class RankingProfilePage : ContentPage
    {
        private const string KEY_PROFILE_ID = "RankingProfileId";
        private string _profileId = string.Empty;

        public RankingProfilePage()
        {
            InitializeComponent();
            _profileId = Preferences.Get(KEY_PROFILE_ID, string.Empty);

            if (!string.IsNullOrEmpty(_profileId))
            {
                _ = LoadExistingProfileAsync(_profileId);
            }
        }

        private async System.Threading.Tasks.Task LoadExistingProfileAsync(string id)
        {
            try
            {
                var profile = await RankingService.GetProfileAsync(id);
                if (profile != null)
                {
                    NickEntry.Text = profile.PlayerName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("B³¹d ³adowania profilu: " + ex.Message);
            }
        }

        private async void OnSaveAndPlayClicked(object sender, EventArgs e)
        {
            var nick = NickEntry.Text?.Trim();
            if (string.IsNullOrEmpty(nick))
            {
                await DisplayAlert("Uwaga", "Wpisz nick", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_profileId))
                _profileId = Guid.NewGuid().ToString("N");

            var profile = new PlayerProfile
            {
                ProfileId = _profileId,
                PlayerName = nick,
                CreatedAt = DateTime.UtcNow.ToString("o"),
                Stats = new PlayerStats()
            };

            try
            {
                await RankingService.CreateOrUpdateProfileAsync(profile);
                Preferences.Set(KEY_PROFILE_ID, _profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("B³¹d zapisu profilu do Firebase: " + ex.Message);
                await DisplayAlert("B³¹d", "Nie uda³o siê zapisaæ profilu zdalnie. Profil zosta³ zapisany lokalnie.", "OK");
                Preferences.Set(KEY_PROFILE_ID, _profileId);
            }

            await Shell.Current.GoToAsync($"///quiz?mode=ranking&profileId={Uri.EscapeDataString(_profileId)}");
        }

        private async void OnShowLeaderboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///scoreboard?source=remote");
        }

        private async void OnDeleteLocalProfileClicked(object sender, EventArgs e)
        {
            bool ok = await DisplayAlert("Usuñ profil", "Czy na pewno usun¹æ lokalnie zapisany profil? (nie usuwa go z Firebase)", "Tak", "Nie");
            if (!ok) return;

            Preferences.Remove(KEY_PROFILE_ID);
            _profileId = string.Empty;
            NickEntry.Text = string.Empty;
            await DisplayAlert("Usuniêto", "Lokalny profil zosta³ usuniêty.", "OK");
        }
    }
}
