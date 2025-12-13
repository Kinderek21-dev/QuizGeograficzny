using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace QuizGeograficzny.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.ScaleTo(0.95, 100);
            await element.ScaleTo(1.0, 100);
        }

        try
        {
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                await Shell.Current.GoToAsync("///gamemode");
                return;
            }

            var availability = await CrossFingerprint.Current.IsAvailableAsync(true);

            if (!availability)
            {
                await DisplayAlert("Uwaga", "Zalogowano testowo (brak biometrii).", "OK");
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                await Shell.Current.GoToAsync("///gamemode");
                return;
            }

            var authRequest = new AuthenticationRequestConfiguration(
                "Uwierzytelnianie",
                "Zaloguj siê, aby rozpocz¹æ quiz.")
            {
                AllowAlternativeAuthentication = true
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(authRequest);

            if (result.Authenticated)
            {
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                await Shell.Current.GoToAsync("///gamemode");
            }
            else
            {
                await DisplayAlert("Niepowodzenie", "Nie uda³o siê uwierzytelniæ.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"Wyst¹pi³ wyj¹tek: {ex.Message}", "OK");
        }
    }
}