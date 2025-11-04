using AdaptiveSystemControl.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class AlertManager
    {
        private readonly List<IAlertNotifier> _notifiers = new List<IAlertNotifier>();

        // Метод для подписки на уведомления
        public void Subscribe(IAlertNotifier notifier)
        {
            if (!_notifiers.Contains(notifier))
            {
                _notifiers.Add(notifier);
            }
        }
        // Метод для отписки от уведомлений
        public void Unsubscribe(IAlertNotifier notifier)
        {
            _notifiers.Remove(notifier);
        }
        // Главный метод для оповещения всех подписчиков
        public async Task NotifyAllAsync(string message, double value)
        {
            // Создаём задачи для всех уведомлений
            var notificationTasks = _notifiers.Select(notifier => notifier.NotifyAsync(message, value));
            // Запускаем все уведомления параллельно и ждём их завершения
            await Task.WhenAll(notificationTasks);
        }

    }
}
/*
 * Объяснение:
Этот класс — как диспетчерская служба. У неё есть:
Список подписчиков (_notifiers) — все, кто хочет получать уведомления.
Метод подписки (Subscribe) — чтобы добавить нового подписчика.
Метод отписки (Unsubscribe) — чтобы удалить подписчика.
Метод оповещения (NotifyAllAsync) — который при наступлении события сообщает всем подписчикам одновременно и ждёт, пока они все обработают уведомление.

 */