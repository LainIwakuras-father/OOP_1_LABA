using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace AdaptiveSystemControl.Services
{
    public class OpcUaSettings
    {
        public string serverUrl { get; set; } = "";
        public string NodeId { get; set; } = "";
    }
    public class MonitoringSettings
    {
        public double criticalHumidityThreshold { get; set; }
        public int monitoringIntervalMs { get; set; }

    }
}// System.Text.JSon не знает о кастомных объектах это не все так просто спарсить!