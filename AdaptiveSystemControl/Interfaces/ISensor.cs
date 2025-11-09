using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Interfaces
{
    public interface ISensor// Используешься шаблон так как некоторые датчики возвращаются разные типы данных
    {
        string SensorType {  get; }
        double ReadValue();
    }
}
