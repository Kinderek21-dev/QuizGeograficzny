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

        private async void OnStandardTapped(object sender, TappedEventArgs e)
        {
            if (sender is VisualElement element)
            {
                await element.ScaleTo(0.95, 100);
                await element.ScaleTo(1.0, 100);
            }


            await Shell.Current.GoToAsync("///difficulty");
        }

        private async void OnSurvivalTapped(object sender, TappedEventArgs e)
        {
            if (sender is VisualElement element)
            {
                await element.ScaleTo(0.95, 100);
                await element.ScaleTo(1.0, 100);
            }


            await Shell.Current.GoToAsync($"///quiz?mode=survival");
        }
    }
}