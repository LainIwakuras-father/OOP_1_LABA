using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class DemoScenarioService : IHostedService
    {
        private readonly MainService _mainService;
        private readonly DataProcessor _dataProcessor;
        private readonly ThresholdCheckStrategy _thresholdStrategy;
        private readonly SuddenJumpDetectionStrategy _jumpStrategy;
        private readonly ILogger<DemoScenarioService> _logger;
        private Timer _demoTimer;

        public DemoScenarioService(
            MainService mainService,
            DataProcessor dataProcessor,
            ThresholdCheckStrategy thresholdStrategy,
            SuddenJumpDetectionStrategy jumpStrategy,
            ILogger<DemoScenarioService> logger)
        {
            _mainService = mainService;
            _dataProcessor = dataProcessor;
            _thresholdStrategy = thresholdStrategy;
            _jumpStrategy = jumpStrategy;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Демонстрационный сценарий запущен");

            // Запускаем демо-сценарий через 5 секунд
            _demoTimer = new Timer(RunDemoScenario, null, 5000, Timeout.Infinite);

            return Task.CompletedTask;
        }
        private void RunDemoScenario(object state)
        {
            _logger.LogInformation("=== НАЧАЛО ДЕМОНСТРАЦИОННОГО СЦЕНАРИЯ ===");
            // 1. Показываем работу пороговой стратегии
            _mainService.ChangeProcessingStrategy(_thresholdStrategy);
            // 2. Через 10 секунд переключаемся на стратегию скачков
            Task.Delay(10000).ContinueWith(t =>
            {
                _mainService.ChangeProcessingStrategy(_jumpStrategy);
                // 3. Ещё через 10 секунд меняем порог
                Task.Delay(10000).ContinueWith(t2 =>
                {
                    _mainService.UpdateThreshold(75.0);
                    // 4. Завершаем демо через 5 секунд
                    Task.Delay(5000).ContinueWith(t3 =>
                    {
                        _logger.LogInformation("=== ДЕМОНСТРАЦИОННЫЙ СЦЕНАРИЙ ЗАВЕРШЁН ===");
                        _logger.LogInformation("Теперь вы можете управлять системой вручную");
                    });
                });
            });
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _demoTimer?.Dispose();
            return Task.CompletedTask;
        }

    }
}
