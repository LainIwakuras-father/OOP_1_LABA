using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSystemControl.Services
{
    public class SuddenJumpDetectionStrategy : IDataProcessingStrategy
    {
        private readonly ILogger<SuddenJumpDetectionStrategy> _logger;
        private readonly double _jumpThreshold; // На сколько единиц должен跳нуть значение, чтобы считаться скачком
        private double? _previousValue = null;
        private readonly Queue<double> _recentValues = new Queue<double>();
        private readonly int _windowSize = 5; // Размер окна для усреднения
        public SuddenJumpDetectionStrategy(ILogger<SuddenJumpDetectionStrategy> logger, IOptions<MonitoringSettings> settings)
        {
            _logger = logger;
            // Можно вынести в конфиг, но для примера зададим фиксированно
            _jumpThreshold = 10.0; // Скачок на 10 единиц считается аномалией
        }
        public Task<bool> ProcessDataAsync(double data)
        {
            bool isAnomaly = false;

            // 1. Проверка на резкий скачок относительно предыдущего значения
            if (_previousValue.HasValue)
            {
                double difference = Math.Abs(data - _previousValue.Value);
                if (difference > _jumpThreshold)
                {
                    _logger.LogWarning("Обнаружен резкий скачок! Текущее значение: {Data}, предыдущее: {Previous}, разница: {Difference}", data, _previousValue, difference);
                    isAnomaly = true;
                }
            }
            _previousValue = data;
            // 2. (Дополнительно) Проверка на отклонение от скользящего среднего
            _recentValues.Enqueue(data);
            if (_recentValues.Count > _windowSize)
            {
                _recentValues.Dequeue();
            }
            if (_recentValues.Count == _windowSize)
            {
                double average = CalculateAverage(_recentValues);
                double deviation = Math.Abs(data - average);
                // Если текущее значение отклоняется от среднего больше чем на 15% - возможная аномалия
                if (deviation > (average * 0.15))
                {
                    _logger.LogWarning("Обнаружено значительное отклонение от среднего! Значение: {Data}, среднее: {Average}, отклонение: {Deviation}", data, average, deviation);
                    isAnomaly = true;
                }
            }
            return Task.FromResult(isAnomaly);
        }
        private double CalculateAverage(IEnumerable<double> values)
        {
            double sum = 0;
            int count = 0;
            foreach (var value in values)
            {
                sum += value;
                count++;
            }
            return sum / count;
        }
    }
}

    
