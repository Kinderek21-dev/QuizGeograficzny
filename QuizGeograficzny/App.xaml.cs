using QuizGeograficzny.Services;
using System.Diagnostics;

namespace QuizGeograficzny
{
    public partial class App : Application
    {
        private readonly ILightSensorService _lightSensorService;

        private const float DARK_THRESHOLD_LUX = 22;

        public static bool LightSensorEnabled { get; set; } = true;

        public App(ILightSensorService lightSensorService)
        {
            InitializeComponent();
            MainPage = new AppShell();

            _lightSensorService = lightSensorService;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                await Shell.Current.GoToAsync("///splash");
            });
        }

        protected override void OnStart()
        {
            base.OnStart();

            Debug.WriteLine("[App] Start aplikacji.");

            if (LightSensorEnabled)
            {
                Debug.WriteLine("[App] Czujnik światła włączony.");
                _lightSensorService.OnLightReadingChanged += OnLightReadingChanged;
                _lightSensorService.StartListening();
            }
            else
            {
                Debug.WriteLine("[App] Czujnik światła wyłączony.");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            if (LightSensorEnabled)
            {
                Debug.WriteLine("[App] Usypianie — wyłączam czujnik.");
                _lightSensorService.StopListening();
                _lightSensorService.OnLightReadingChanged -= OnLightReadingChanged;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (LightSensorEnabled)
            {
                Debug.WriteLine("[App] Wznowienie — włączam czujnik.");
                _lightSensorService.OnLightReadingChanged += OnLightReadingChanged;
                _lightSensorService.StartListening();
            }
        }

        private void OnLightReadingChanged(float currentLux)
        {
            if (!LightSensorEnabled)
                return;

            AppTheme newTheme = currentLux < DARK_THRESHOLD_LUX
                ? AppTheme.Dark
                : AppTheme.Light;

            Debug.WriteLine($"[App] Odczyt: {currentLux} lux. Wybrany motyw: {newTheme}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current.UserAppTheme != newTheme)
                {
                    Debug.WriteLine($"[App] Zmieniam motyw na: {newTheme}");
                    Application.Current.UserAppTheme = newTheme;
                }
            });
        }
    }
}
