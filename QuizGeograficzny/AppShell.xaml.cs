namespace QuizGeograficzny
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var settings = new ToolbarItem
            {
                Text = "Ustawienia",
                Order = ToolbarItemOrder.Secondary 
            };
            settings.Clicked += async (s, e) => await Shell.Current.GoToAsync("///settings");
            this.ToolbarItems.Add(settings);
        }
    }

}
