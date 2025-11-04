// НАШЕЛ необходимые БИБЛИОТЕКИ 
using AdaptiveSystemControl.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;
using static System.Collections.Specialized.BitVector32;

namespace AdaptiveSystemControl.Services
{
    public class OpcUaDataProvider : IDataProvider
    {
        private readonly ILogger<OpcUaDataProvider> _logger;
        private readonly OpcUaSettings _settings;
        // Конструктор. Сюда система "положит" нужные настройки и логгер
        public OpcUaDataProvider(IOptions<OpcUaSettings> settings, ILogger<OpcUaDataProvider> logger)
        {
            _logger = logger;
            _settings = settings.Value; // Достаём сами настройки из контейнера
            _logger.LogInformation("OPC UA провайдер создан для сервера: {ServerUrl}", _settings.serverUrl);
        }
        public async Task<double> ReadValueAsync()
        {
            try
            {
                // 1. Создаём настройки для подключения
                var applicationDescription = new ApplicationDescription
                {
                    ApplicationName = "AdaptiveControlSystem Client",
                    ApplicationUri = "urn:localhost:AdaptiveControlSystem",
                    ApplicationType = ApplicationType.Client
                };

                // Создаем endpoint с отключенной безопасностью ВАЖНАЯ ШТУКА
                var endpointDescription = new EndpointDescription
                {
                    EndpointUrl = _settings.serverUrl,
                    SecurityMode = MessageSecurityMode.None,
                    SecurityPolicyUri = SecurityPolicyUris.None
                };

                // 2. Создаём и запускаем подключение к серверу
                var channel = new ClientSessionChannel( // в файле Павловича -  UaTcpSessionChannel, по результатом поиска сейчас используется ClientSessionChannel
               applicationDescription,
               null, // Без сертификата
               new AnonymousIdentity(), // Анонимный вход
               endpointDescription); // Используем endpoint description вместо URL
                {
                    await channel.OpenAsync();
                    _logger.LogInformation("Сессия создана: {SessionId}", channel.SessionId);
                    _logger.LogInformation("Считываем данные с {NodeId}", _settings.NodeId);

                    // 3. Читаем значение с конкретного "датчика" (NodeId)
                    var readRequest = new ReadRequest
                    {
                        NodesToRead = [new ReadValueId { NodeId = NodeId.Parse(_settings.NodeId), AttributeId = AttributeIds.Value }]
                    };

                    

                    var readResponse = await channel.ReadAsync(readRequest);
                    // 4. Если ответ успешный, возвращаем значение
                    if (readResponse.Results[0].StatusCode == StatusCodes.Good)
                    {
                        double value = (double)readResponse.Results[0].Value;
                        _logger.LogDebug("Прочитано значение из OPC UA: {Value}", value);
                        return value;
                    }
                    else
                    {
                        _logger.LogError("Ошибка чтения из OPC UA. Статус: {StatusCode}", readResponse.Results[0].StatusCode);
                        return double.NaN; // Возвращаем специальное значение "Не число" (Not a Number) для обозначения ошибки
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подключения или чтения из OPC UA сервера {ServerUrl}", _settings.serverUrl);
                return double.NaN;
            }
        }
    }
}
