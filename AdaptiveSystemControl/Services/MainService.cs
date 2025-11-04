using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class MainService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly MonitoringService _monitoringService;
        private readonly DataProcessor _dataProcessor;
        private readonly AlertManager _alertManager;
        private readonly ILogger<MainService> _logger;
        private readonly IOptions<MonitoringSettings> _settings;

        public MainService(
            IHostApplicationLifetime appLifetime,
            MonitoringService monitoringService,
            DataProcessor dataProcessor,
            AlertManager alertManager,
            ILogger<MainService> logger,
            IOptions<MonitoringSettings> settings)
        {
            _appLifetime = appLifetime;
            _monitoringService = monitoringService;
            _dataProcessor = dataProcessor;
            _alertManager = alertManager;
            _logger = logger;
            _settings = settings;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("=== АДАПТИВНАЯ СИСТЕМА УПРАВЛЕНИЯ ЗАПУЩЕНА ===");
            _logger.LogInformation("Текущие настройки:");
            _logger.LogInformation("  - Интервал мониторинга: {Interval} мс", _settings.Value.monitoringIntervalMs);
            _logger.LogInformation("  - Критический порог: {Threshold}", _settings.Value.criticalHumidityThreshold);
            _logger.LogInformation("  - Текущая стратегия: {Strategy}", _dataProcessor.GetCurrentStrategyName());
            _logger.LogInformation("Для остановки нажмите Ctrl+C");
            // Запускаем сервис мониторинга
            await _monitoringService.StartAsync(cancellationToken);
            // Регистрируем обработчик graceful shutdown
            _appLifetime.ApplicationStopping.Register(OnShutdown);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка системы...");
            await _monitoringService.StopAsync(cancellationToken);
            _logger.LogInformation("=== СИСТЕМА ОСТАНОВЛЕНА ===");
        }
        private void OnShutdown()
        {
            _logger.LogInformation("Получен сигнал остановки. Завершаем работу...");
        }
        // Метод для смены стратегии обработки
        public void ChangeProcessingStrategy(IDataProcessingStrategy newStrategy)
        {
            _dataProcessor.SetStrategy(newStrategy);
            _logger.LogInformation("Стратегия изменена на: {Strategy}", newStrategy.GetType().Name);
        }
        // Метод для изменения порогового значения
        public void UpdateThreshold(double newThreshold)
        {
            // Для простоты демонстрации создаём новый экземпляр настроек
            // В реальной системе нужно было бы использовать более сложный механизм
            _logger.LogInformation("Критический порог изменён: {Old} -> {New}",
                _settings.Value.criticalHumidityThreshold, newThreshold);
            // Обновляем настройки (в реальной системе это должно быть потокобезопасно)
            _settings.Value.criticalHumidityThreshold = newThreshold;
        }

    }
}
