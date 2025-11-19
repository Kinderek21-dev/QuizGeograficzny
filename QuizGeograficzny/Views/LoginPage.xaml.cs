using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace QuizGeograficzny.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }


    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                await Shell.Current.GoToAsync("///gamemode");
                return;
            }

           
            var availability = await CrossFingerprint.Current.IsAvailableAsync(true);

            if (!availability)
            {
                await DisplayAlert("Uwaga", "Zalogowano testowo.", "OK");
                await Shell.Current.GoToAsync("///gamemode");
                return;
            }

            var authRequest = new AuthenticationRequestConfiguration(
                "Uwierzytelnianie",
                "Zaloguj siê odciskiem palca, aby rozpocz¹æ quiz.")
            {
                AllowAlternativeAuthentication = true
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(authRequest);

            if (result.Authenticated)
            {
                await DisplayAlert("Sukces", "Pomyœlnie zalogowano!", "OK");
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
