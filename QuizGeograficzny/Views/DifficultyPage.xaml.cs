namespace QuizGeograficzny.Views;

public partial class DifficultyPage : ContentPage
{
    public DifficultyPage()
    {
        InitializeComponent();
    }


    private async void OnDifficultySelected(object sender, TappedEventArgs e)
    {

        var difficulty = e.Parameter as string ?? "³atwy";


        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }

        await Shell.Current.GoToAsync($"///quiz?difficulty={Uri.EscapeDataString(difficulty)}");
    }
}