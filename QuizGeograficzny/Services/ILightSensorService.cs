using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGeograficzny.Services
{
    public interface ILightSensorService
    {
        event Action<float> OnLightReadingChanged;
        void StartListening();
        void StopListening();
    }
}