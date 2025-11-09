using AdaptiveSystemControl.Interfaces;
using AdaptiveSystemControl.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AdaptiveSystemControl.Tests
{
    [TestClass]
    public class SensorFactoryTests
    {
        [TestMethod]
        public void CreateSensor_WhenTemperatureType_ReturnsTemperatureSensor()
        {
            var SensorTemp = SensorFactory.CreateSensor("Temperature");

            Assert.IsInstanceOfType(SensorTemp, typeof(TemperatureSensor));

        }
        [TestMethod]
        public void CreateSensor_WhenHumidityType_ReturnsHumiditySensor()
        {
            var SensorTemp = SensorFactory.CreateSensor("Humidity");

            Assert.IsInstanceOfType(SensorTemp, typeof(HumiditySensor));

        }

    }

}
