using AdaptiveSystemControl.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Sensors
{
    public class SensorFactory
    {
        public static ISensor CreateSensor(string sensorType)
        {
        // проверка на тип прежде чем создать датчик
            
                switch (sensorType.ToLower())
                {
                    case "humidity":
                        return new HumiditySensor();
                    case "temperature":
                        return new TemperatureSensor();
                    default:
                        throw new ArgumentException($"Unknown sensor type: {sensorType}");
                }
            
           
        }
    }
}
