namespace QuizGeograficzny
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("quiz", typeof(Views.QuizPage));
            Routing.RegisterRoute("result", typeof(Views.ResultPage));
            Routing.RegisterRoute("scoreboard", typeof(Views.ScoreboardPage));
            var settings = new ToolbarItem
            {
                Text = "⚙️",

                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };

            settings.Clicked += async (s, e) =>
            {
                await Shell.Current.GoToAsync("///settings");
            };

            this.ToolbarItems.Add(settings);
        }
    }
}