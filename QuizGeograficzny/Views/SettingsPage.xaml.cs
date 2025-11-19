using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Views
{
    public partial class SettingsPage : ContentPage
    {
        private const string KEY_AUTO_THEME = "AutoThemeEnabled";
        private const string KEY_MANUAL_THEME = "ManualTheme";

        public SettingsPage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            bool auto = Preferences.Get(KEY_AUTO_THEME, true);
            AutoThemeSwitch.IsToggled = auto;

            string manual = Preferences.Get(KEY_MANUAL_THEME, "System");
            ThemePicker.SelectedIndex = manual == "Light" ? 1 : manual == "Dark" ? 2 : 0;

            ThemePicker.IsEnabled = !auto;
        }

        private void OnAutoThemeToggled(object sender, ToggledEventArgs e)
        {
            ThemePicker.IsEnabled = !e.Value;
        }

        private void OnThemePickerChanged(object sender, EventArgs e) { /* nic */ }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            Preferences.Set(KEY_AUTO_THEME, AutoThemeSwitch.IsToggled);

            var manual = ThemePicker.SelectedIndex switch
            {
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };
            Preferences.Set(KEY_MANUAL_THEME, manual);

            if (!AutoThemeSwitch.IsToggled)
            {
                Application.Current.UserAppTheme = manual switch
                {
                    "Light" => AppTheme.Light,
                    "Dark" => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
            }

            await DisplayAlert("Zapisano", "Ustawienia zapisane.", "OK");
        }
    }
}
