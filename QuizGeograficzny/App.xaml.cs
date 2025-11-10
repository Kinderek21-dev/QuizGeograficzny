using QuizGeograficzny.Services;
using System.Diagnostics; 

namespace QuizGeograficzny
{
    public partial class App : Application
    {
        private readonly ILightSensorService _lightSensorService;

        private const float DARK_THRESHOLD_LUX = 22;

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
            Debug.WriteLine("[App] Uruchamiam nasłuch czujnika.");
            _lightSensorService.OnLightReadingChanged += OnLightReadingChanged;
            _lightSensorService.StartListening();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _lightSensorService.StopListening();
            _lightSensorService.OnLightReadingChanged -= OnLightReadingChanged;
        }

        protected override void OnResume()
        {
            base.OnResume();
            _lightSensorService.OnLightReadingChanged += OnLightReadingChanged;
            _lightSensorService.StartListening();
        }

        private void OnLightReadingChanged(float currentLux)
        {
            AppTheme newTheme = currentLux < DARK_THRESHOLD_LUX ? AppTheme.Dark : AppTheme.Light;

            Debug.WriteLine($"[App] Odczyt: {currentLux} lux. Próg: {DARK_THRESHOLD_LUX}. Wybrany motyw: {newTheme}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current.UserAppTheme != newTheme)
                {
                    Debug.WriteLine($"[App] ZMIENIAM MOTYW na: {newTheme}");
                    Application.Current.UserAppTheme = newTheme;
                }
            });
        }
    }
}