using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace AdaptiveSystemControl.Services
{
    public class CircuitBreaker
    {
        private readonly ILogger<CircuitBreaker> _logger;
        //Гибрид 

        //Процент ошибок
        private int _requestCount;
        private int _errorCount;
        private double _errorRateThreshold;

        //Оценка
        private TimeSpan _responseTimeThreshold;
        private List<TimeSpan> _responseTimes;

        // Состояние и автоматическое восстановление
        private bool _isCircuitOpen;
        private DateTime _circuitOpenedAt;
        private readonly TimeSpan _resetTimeout;

        public CircuitBreaker(
            ILogger<CircuitBreaker> logger,
            double errorRateThreshold,
            TimeSpan responseTimeThreshold,
            TimeSpan resetTimeout = default)
        {
            _logger = logger;

            _errorRateThreshold = errorRateThreshold;

            _responseTimeThreshold = responseTimeThreshold;
            _responseTimes = new List<TimeSpan>();

            _resetTimeout = resetTimeout == default ? TimeSpan.FromMinutes(5) : resetTimeout;
            _responseTimes = new List<TimeSpan>();
            _isCircuitOpen = false;
        }
        public void RecordRequest(bool success, TimeSpan responseTime)
        {
            // Если цепь открыта, проверяем возможность автоматического восстановления
            if (_isCircuitOpen)
            {
                if (DateTime.UtcNow - _circuitOpenedAt >= _resetTimeout)
                {
                    CloseCircuit();
                }
                else
                {
                    return; // Пропускаем запись, пока цепь открыта
                }
            }

            _requestCount++;
            _responseTimes.Add(responseTime);

            // Ограничиваем размер истории
            if (_responseTimes.Count > 20)
            {
                _responseTimes.RemoveAt(0);
            }

            if (!success)
            {
                _errorCount++;
            }

            // Проверяем условия для открытия цепи
            bool shouldOpen = false;

            // Проверка процента ошибок (минимум 5 запросов для статистики)
            if (_requestCount >= 5)
            {
                double currentErrorRate = (double)_errorCount / _requestCount;
                if (currentErrorRate > _errorRateThreshold)
                {
                    _logger.LogWarning($"Error rate threshold exceeded: {currentErrorRate:P2} > {_errorRateThreshold:P2}");
                    shouldOpen = true;
                }
            }

            // Проверка времени ответа (минимум 3 измерения)
            if (_responseTimes.Count >= 3 && _responseTimes.Average(ts => ts.TotalMilliseconds) > _responseTimeThreshold.TotalMilliseconds)
            {
                _logger.LogWarning($"Response time threshold exceeded: {_responseTimes.Average(ts => ts.TotalMilliseconds):F2}ms > {_responseTimeThreshold.TotalMilliseconds}ms");
                shouldOpen = true;
            }

            if (shouldOpen && !_isCircuitOpen)
            {
                OpenCircuit();
            }
        }

        private void OpenCircuit()
        {
            _isCircuitOpen = true;
            _circuitOpenedAt = DateTime.UtcNow;
            _logger.LogInformation($"Circuit Breaker Opened at {DateTime.UtcNow}");
        }

        private void CloseCircuit()
        {
            _isCircuitOpen = false;
            Reset();
            _logger.LogInformation($"Circuit Breaker Closed at {DateTime.UtcNow}");
        }

        public bool IsCircuitClosed()
        {
            return !_isCircuitOpen;
        }

        public void Reset()
        {
            _errorCount = 0;
            _requestCount = 0;
            _responseTimes.Clear();
        }

        public string GetStatus()
        {
            return _isCircuitOpen ?
                $"Open (since {_circuitOpenedAt:HH:mm:ss}, auto-reset in {_resetTimeout - (DateTime.UtcNow - _circuitOpenedAt):mm\\:ss})" :
                "Closed";
        }
    }
}
