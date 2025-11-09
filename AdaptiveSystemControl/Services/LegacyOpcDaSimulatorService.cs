
using AdaptiveSystemControl.Interfaces;
using AdaptiveSystemControl.Sensors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace AdaptiveSystemControl.Services
{
    public class LegacyOpcDaSimulatorService : IDataProvider
    {
        
        private readonly ILogger<LegacyOpcDaSimulatorService> _logger;
        private readonly Random _random = new Random();

        public LegacyOpcDaSimulatorService(ILogger<LegacyOpcDaSimulatorService> logger)
        {

            _logger = logger;
            _logger.LogInformation("Симулятор Legacy OPC DA сервиса создан.");
        }
        public Task<double> ReadValueAsync()
        {
            try
            {
                // Имитация задержки старого оборудования (от 100 до 500 мс)
                int delayMs = _random.Next(100, 500);
                Task.Delay(delayMs).Wait(); // Имитируем задержку блокирующим вызовом, как в старых системах
                // Генерируем "давление" в диапазоне от 70.0 до 85.0
                double simulatedValue = 70.0 + _random.NextDouble() * 15.0;
                // С небольшим шансом сгенерировать аномалию (скачок до 100)
                if (_random.NextDouble() < 0.05) // 5% шанс
                {
                    simulatedValue = 100.0;
                    _logger.LogWarning("(LEGACY SIM) Сгенерирована аномалия давления: {Value}", simulatedValue);
                }
                else
                {
                    _logger.LogDebug("(LEGACY SIM) Сгенерировано значение: {Value}", simulatedValue);
                }
                return Task.FromResult(simulatedValue);




            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в симуляторе OPC DA");
                return Task.FromResult(double.NaN);
            }
        }
    }
}
