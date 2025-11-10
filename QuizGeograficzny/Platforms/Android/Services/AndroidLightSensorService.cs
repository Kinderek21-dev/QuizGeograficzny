using Android.Content;
using Android.Hardware;
using QuizGeograficzny.Services;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace QuizGeograficzny.Platforms.Android.Services
{
    public class AndroidLightSensorService : Java.Lang.Object, ILightSensorService, ISensorEventListener
    {
        private SensorManager _sensorManager;
        private Sensor _lightSensor;

        public event Action<float> OnLightReadingChanged;

        public AndroidLightSensorService()
        {
            _sensorManager = (SensorManager)Platform.AppContext.GetSystemService(Context.SensorService);
            _lightSensor = _sensorManager.GetDefaultSensor(SensorType.Light);

            if (_lightSensor == null)
            {
                Debug.WriteLine("[LightSensor] Czujnik światła NIE ZOSTAŁ ZNALEZIONY.");
            }
            else
            {
                Debug.WriteLine("[LightSensor] Czujnik światła znaleziony.");
            }
        }

        public void StartListening()
        {
            if (_lightSensor != null)
            {
                _sensorManager.RegisterListener(this, _lightSensor, SensorDelay.Normal);
            }
        }

        public void StopListening()
        {
            _sensorManager.UnregisterListener(this);
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            if (e?.Sensor?.Type == SensorType.Light)
            {
                float lux = e.Values[0];
                Debug.WriteLine($"[LightSensor] Odczyt czujnika: {lux} lux");
                OnLightReadingChanged?.Invoke(lux);
            }
        }

        public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
        {
        }
    }
}