using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Views
{
    public partial class RankingModePage : ContentPage
    {
        private const string KEY_PROFILE_ID = "LocalProfileId";

        public RankingModePage()
        {
            InitializeComponent();
        }


        private async void OnPlayRankingTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement element)
            {
                await element.ScaleTo(0.95, 100);
                await element.ScaleTo(1.0, 100);
            }

          
            string profileId = Preferences.Get(KEY_PROFILE_ID, string.Empty);

            if (string.IsNullOrEmpty(profileId))
            {
                await DisplayAlert("Brak profilu", "Najpierw utwórz profil w sekcji 'Profil i Ranking globalny', aby graæ w trybie rankingowym.", "OK");

                return;
            }

         
            await Shell.Current.GoToAsync($"///quiz?mode=ranking&profileId={profileId}");
        }


        private async void OnOpenProfileTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement element)
            {
                await element.ScaleTo(0.95, 100);
                await element.ScaleTo(1.0, 100);
            }

            await Shell.Current.GoToAsync("///rankingprofile");
        }
    }
}