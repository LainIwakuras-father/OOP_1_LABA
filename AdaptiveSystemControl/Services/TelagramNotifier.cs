using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdaptiveSystemControl.Services
{
    public class TelagramNotifier : IAlertSubscriber
    {
        private readonly ILogger<TelagramNotifier> _logger;
        public TelagramNotifier(ILogger<TelagramNotifier> logger) 
        {
            _logger = logger;
        }

        public async Task Alert(string message)
        {
            int delayMs = new Random().Next(500, 1000);
            await Task.Delay(delayMs);
            // Имитация отправки в Telegram
            Console.WriteLine($"TELEGRAM: {message}");
            _logger.LogInformation("Telegram notification sent: {Message}", message);

            // Здесь мог бы быть реальный код для отправки в Telegram API
            // await _telegramClient.SendMessageAsync(chatId, message);
        }
    }
}
