using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdaptiveSystemControl.Interfaces;

namespace AdaptiveSystemControl.Sensors
{
    public class HumiditySensor : ISensor
    {
        private Random random = new();
        public string SensorType => "humidity";

        public double ReadValue()
        {
            return random.Next(0, 100);// ЗАТЫЧКА НО НО МОЖНО Читать значение из OPC UA сервера!!!
        }    
    }
}
