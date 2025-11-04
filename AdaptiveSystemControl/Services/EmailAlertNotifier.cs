using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class EmailAlertNotifier : IAlertNotifier
    {
        private readonly ILogger<EmailAlertNotifier> _logger;

        public EmailAlertNotifier(ILogger<EmailAlertNotifier> logger)
        {
            _logger = logger;
        }
        public async Task NotifyAsync(string message, double value)
        {
            // Имитация отправки email (задержка 0.5-1 секунда)
            int delayMs = new Random().Next(500, 1000);
            await Task.Delay(delayMs);
            _logger.LogWarning("EMAIL ОТПРАВЛЕН: {Message}. Значение: {Value}. (Задержка: {Delay}мс)", message, value, delayMs);
        }

    }
}
/*
 * Объяснение:
Этот класс — как курьер, который не просто записывает событие, а бежит сообщать о нём другому человеку (отправляет email). Он имитирует реальную задержку сети, чтобы показать, что уведомления могут работать асинхронно и занимать разное время.

 */