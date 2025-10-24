namespace QuizGeograficzny.Views;

public partial class DifficultyPage : ContentPage
{
    public DifficultyPage()
    {
        InitializeComponent();
    }

    private async void GoToQuiz(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var difficulty = btn?.Text ?? "³atwy";
        await Shell.Current.GoToAsync($"///quiz?difficulty={Uri.EscapeDataString(difficulty)}");
    }
}
