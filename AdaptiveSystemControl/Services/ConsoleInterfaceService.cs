using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class ConsoleInterfaceService : IHostedService
    {
        private readonly MainService _mainService;
        private readonly DataProcessor _dataProcessor;
        private readonly ThresholdCheckStrategy _thresholdStrategy;
        private readonly SuddenJumpDetectionStrategy _jumpStrategy;
        private readonly ILogger<ConsoleInterfaceService> _logger;
        private CancellationTokenSource _cts;
        public ConsoleInterfaceService(
            MainService mainService,
            DataProcessor dataProcessor,
            ThresholdCheckStrategy thresholdStrategy,
            SuddenJumpDetectionStrategy jumpStrategy,
            ILogger<ConsoleInterfaceService> logger)
        {
            _mainService = mainService;
            _dataProcessor = dataProcessor;
            _thresholdStrategy = thresholdStrategy;
            _jumpStrategy = jumpStrategy;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Запускаем обработку команд в отдельном потоке
            Task.Run(() => ProcessCommands(_cts.Token), _cts.Token);
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }
        private async Task ProcessCommands(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken); // Не блокируем поток полностью
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);

                        switch (key.Key)
                        {
                            case ConsoleKey.D1:
                                _mainService.ChangeProcessingStrategy(_thresholdStrategy);
                                break;

                            case ConsoleKey.D2:
                                _mainService.ChangeProcessingStrategy(_jumpStrategy);
                                break;
                            case ConsoleKey.Add:
                                Console.Write("Введите новое пороговое значение: ");
                                if (double.TryParse(Console.ReadLine(), out double newThreshold))
                                {
                                    _mainService.UpdateThreshold(newThreshold);
                                }
                                else
                                {
                                    _logger.LogError("Некорректное значение!");
                                }
                                break;
                            case ConsoleKey.Escape:
                                _logger.LogInformation("Завершение работы...");
                                Environment.Exit(0);
                                break;
                            case ConsoleKey.H:
                                PrintHelp();
                                break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Запрос на отмену
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в обработчике команд");
                }
            }
        }
        private void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("=== КОМАНДЫ УПРАВЛЕНИЯ ===");
            Console.WriteLine("1 - Использовать пороговую стратегию");
            Console.WriteLine("2 - Использовать стратегию обнаружения скачков");
            Console.WriteLine("+ - Изменить пороговое значение");
            Console.WriteLine("h - Показать справку");
            Console.WriteLine("ESC - Выход");
            Console.WriteLine();
        }

    }
}
