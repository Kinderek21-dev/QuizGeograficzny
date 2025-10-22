namespace QuizGeograficzny.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        _ = LoadAppAsync();
    }

    private async Task LoadAppAsync()
    {
        await Task.Delay(2000); 
        await Shell.Current.GoToAsync("///login", true);
    }
}
