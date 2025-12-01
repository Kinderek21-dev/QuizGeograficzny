using System;
using Microsoft.Maui.Controls;

namespace QuizGeograficzny.Views
{
    public partial class RankingModePage : ContentPage
    {
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
   
            await Shell.Current.GoToAsync("///quiz?mode=ranking");
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