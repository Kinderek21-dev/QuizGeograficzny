using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGeograficzny.Services
{
    public class DefaultLightSensorService : ILightSensorService
    {
        public event Action<float> OnLightReadingChanged;
        public void StartListening() { }
        public void StopListening() { }
    }
}