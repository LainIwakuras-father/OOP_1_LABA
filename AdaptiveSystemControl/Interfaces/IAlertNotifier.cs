using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Interfaces
{
    public interface IAlertNotifier
    {
        Task NotifyAsync(string message, double value);

    }
}
