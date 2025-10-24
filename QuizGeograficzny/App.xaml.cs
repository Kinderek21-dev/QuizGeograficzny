namespace QuizGeograficzny
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100); 
                await Shell.Current.GoToAsync("///splash");
            });
        }
    }
}
