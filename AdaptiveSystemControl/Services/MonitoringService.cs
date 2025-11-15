using AdaptiveSystemControl.Interfaces;
using AdaptiveSystemControl.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class MonitoringService : IHostedService
    {
        private readonly IDataProvider _dataProvider;
        private readonly DataProcessor _dataProcessor; // Используем наш процессор
        private readonly AlertManager _alertManager; // Добавляем AlertManager
        private readonly ILogger<MonitoringService> _logger;
        private readonly int _monitoringIntervalMs;
        private Timer _timer;

        private readonly CircuitBreaker _circuitBreaker;
        //private readonly MonitoringSettings _settings;
        // Изменяем конструктор: принимаем DataProcessor вместо конкретной стратегии
        public MonitoringService(
            IDataProvider dataProvider,
            DataProcessor dataProcessor, // Внедряем процессор
            AlertManager alertManager, // Внедряем AlertManager
            ILogger<MonitoringService> logger,
            IOptions<MonitoringSettings> settings,
            CircuitBreaker circuitBreaker)
        {
            _dataProvider = dataProvider;
            _dataProcessor = dataProcessor;
            _alertManager = alertManager;
            _logger = logger;
            _monitoringIntervalMs = settings.Value.monitoringIntervalMs;

            _circuitBreaker = circuitBreaker;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис мониторинга запущен. Интервал: {Interval} мс", _monitoringIntervalMs);
            _timer = new Timer(DoWork, null, 0, _monitoringIntervalMs);
            return Task.CompletedTask;
        }
        private async void DoWork(object state)
        {   var stop = Stopwatch.StartNew();
            try
            {
                // 1. Читаем данные
                double rawData = await _dataProvider.ReadValueAsync();
                _logger.LogInformation("Получены сырые данные: {RawData}", rawData);
                // 2. ОБРАБАТЫВАЕМ ДАННЫЕ ЧЕРЕЗ ПРОЦЕССОР (который использует стратегию)
                bool isAnomaly = await _dataProcessor.ProcessDataAsync(rawData);
                // 3. Если обнаружена аномалия - реагируем
                if (isAnomaly)
                {
                    string alertMessage = $"Обнаружена аномалия! Значение: {rawData}";
                    _logger.LogCritical("!!! АНОМАЛИЯ ОБНАРУЖЕНА !!! Значение: {Value}", rawData);
                    // ОПОВЕЩАЕМ ВСЕХ ПОДПИСЧИКОВ
                    await _alertManager.NotifyAllAsync(alertMessage, rawData);

                    await _alertManager.AlertAsync(alertMessage);

                }
                else
                {
                    _logger.LogInformation("Данные в норме.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в процессе мониторинга");
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

    }
}
