using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdaptiveSystemControl.Services
{
    public class DataProcessor
    {
        private IDataProcessingStrategy _strategy;
        private readonly ILogger<DataProcessor> _logger;

        // Конструктор.Можно установить стратегию по умолчанию
        public DataProcessor(ILogger<DataProcessor> logger)
        {
            _logger = logger;
            _strategy = null; // Пока стратегия не установлена
        }
        // Метод для смены стратегии на лету
        public void SetStrategy(IDataProcessingStrategy strategy)
        {
            _logger.LogInformation("Смена стратегии обработки данных на {StrategyName}", strategy.GetType().Name);
            _strategy = strategy;
        }
        // Главный метод обработки
        public async Task<bool> ProcessDataAsync(double data)
        {
            if (_strategy == null)
            {
                _logger.LogError("Не выбрана стратегия обработки данных!");
                return false;
            }
            return await _strategy.ProcessDataAsync(data);
        }
        public string GetCurrentStrategyName()
        {
            return _strategy?.GetType().Name ?? "Не выбрана";
        }


    }
}
