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
                await Shell.Current.GoToAsync("///difficulty");
                return;
            }

           
            var availability = await CrossFingerprint.Current.IsAvailableAsync(true);

            if (!availability)
            {
                await DisplayAlert("Uwaga", "Zalogowano testowo.", "OK");
                await Shell.Current.GoToAsync("///difficulty");
                return;
            }

            var authRequest = new AuthenticationRequestConfiguration(
                "Uwierzytelnianie",
                "Zaloguj si� odciskiem palca, aby rozpocz�� quiz.")
            {
                AllowAlternativeAuthentication = true
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(authRequest);

            if (result.Authenticated)
            {
                await DisplayAlert("Sukces", "Pomy�lnie zalogowano!", "OK");
                await Shell.Current.GoToAsync("///difficulty");
            }
            else
            {
                await DisplayAlert("Niepowodzenie", "Nie uda�o si� uwierzytelni�.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B��d", $"Wyst�pi� wyj�tek: {ex.Message}", "OK");
        }
    }

}
