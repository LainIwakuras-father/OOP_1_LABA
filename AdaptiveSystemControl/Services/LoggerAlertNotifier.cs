using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdaptiveSystemControl.Services
{
    public class LoggerAlertNotifier : IAlertNotifier
    {
        private readonly ILogger<LoggerAlertNotifier> _logger;

        public LoggerAlertNotifier(ILogger<LoggerAlertNotifier> logger)
        {
            _logger = logger;
        }
        public Task NotifyAsync(string message, double value)
        {

            _logger.LogCritical("УВЕДОМЛЕНИЕ: {Message}. Значение: {Value}", message, value);
            return Task.CompletedTask;

        }
    }
}
/*
 * Объяснение:
Этот класс — как ответственный секретарь, который записывает
все важные события в журнал. Он получает уведомление и аккуратно
заносит его в лог с пометкой CRITICAL.
 */