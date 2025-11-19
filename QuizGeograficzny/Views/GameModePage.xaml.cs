using System;
using Microsoft.Maui.Controls;

namespace QuizGeograficzny.Views
{
    public partial class GameModePage : ContentPage
    {
        public GameModePage()
        {
            InitializeComponent();
        }

        private async void OnStandardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///difficulty");
        }

        private async void OnSurvivalClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///quiz?mode=survival");
        }
    }
}
