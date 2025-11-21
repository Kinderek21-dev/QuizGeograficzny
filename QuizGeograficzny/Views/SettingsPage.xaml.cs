using Microsoft.Maui.Storage;

namespace QuizGeograficzny.Views;

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
        ThemePicker.Opacity = auto ? 0.5 : 1.0;
    }

    private void OnAutoThemeToggled(object sender, ToggledEventArgs e)
    {
        ThemePicker.IsEnabled = !e.Value;
        ThemePicker.Opacity = e.Value ? 0.5 : 1.0;
        App.LightSensorEnabled = e.Value;
    }

    private void OnThemePickerChanged(object sender, EventArgs e)
    {
    }

    private async void OnSaveTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }

        Preferences.Set(KEY_AUTO_THEME, AutoThemeSwitch.IsToggled);

        var manual = ThemePicker.SelectedIndex switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "System"
        };
        Preferences.Set(KEY_MANUAL_THEME, manual);

        App.LightSensorEnabled = AutoThemeSwitch.IsToggled;

        if (!AutoThemeSwitch.IsToggled)
        {
            Application.Current.UserAppTheme = manual switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }

        await DisplayAlert("Zapisano", "Ustawienia zosta³y zaktualizowane.", "OK");
    }
}