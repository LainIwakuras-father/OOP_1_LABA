using AdaptiveSystemControl.Interfaces;
using AdaptiveSystemControl.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting..");
// 1. Создаём "построителя" хоста
var builder = Host.CreateApplicationBuilder(args);
// 2. Говорим, где брать настройки (appsettings.json)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
// 3. Настраиваем сервисы (наших "игроков")
builder.Services.Configure<OpcUaSettings>(builder.Configuration.GetSection("OpcUaSettings"));
builder.Services.Configure<MonitoringSettings>(builder.Configuration);
// 4. РЕГИСТРИРУЕМ НАШИ ИСТОЧНИКИ ДАННЫХ
// Если нужно использовать OPC UA - раскомментируйте следующую строку:
builder.Services.AddTransient<IDataProvider, OpcUaDataProvider>();
// Если нужно использовать симулятор OPC DA - раскомментируйте следующую строку:
//builder.Services.AddTransient<IDataProvider, LegacyOpcDaSimulatorService>();

// DataProcessor должен быть один
// 6. Устанавливаем стратегию по умолчанию для DataProcessor
// Для этого используем фабрику для DataProcessor
//builder.Services.AddSingleton(provider =>
//{
//    var logger = provider.GetRequiredService<ILogger<DataProcessor>>();
//    var processor = new DataProcessor(logger);
//    // По умолчанию используем пороговую проверку
//    var defaultStrategy = provider.GetRequiredService<ThresholdCheckStrategy>();
//    processor.SetStrategy(defaultStrategy);
//    return processor;
//});


// 5.РЕГИСТРИРУЕМ НАШИ СТРАТЕГИИ И ОБРАБОТЧИК
builder.Services.AddTransient<ThresholdCheckStrategy>();
builder.Services.AddTransient<SuddenJumpDetectionStrategy>();
builder.Services.AddSingleton<DataProcessor>();

//// Разные способы регистрации
//services.AddTransient<IOpcUaService, OpcUaService>(); // Новый экземпляр каждый раз
//services.AddScoped<IOpcUaService, OpcUaService>();    // Один экземпляр на запрос
//services.AddSingleton<IOpcUaService, OpcUaService>(); // Один экземпляр на все приложение
// 7. РЕГИСТРИРУЕМ КОМПОНЕНТЫ СИСТЕМЫ ОПОВЕЩЕНИЯ
builder.Services.AddTransient<IAlertNotifier, LoggerAlertNotifier>();
builder.Services.AddTransient<IAlertNotifier, EmailAlertNotifier>();
builder.Services.AddSingleton<AlertManager>();
// 8. Настраиваем AlertManager: подписываем все уведомления при создании
builder.Services.AddSingleton(provider =>
{
    var alertManager = new AlertManager();
    // Получаем все зарегистрированные реализации IAlertNotifier
    var notifiers = provider.GetServices<IAlertNotifier>();
    foreach (var notifier in notifiers)
    {
        alertManager.Subscribe(notifier);
    }
    return alertManager;
});
/*
 * Важный момент:
Мы используем GetServices<IAlertNotifier>(), 
чтобы получить все зарегистрированные реализации интерфейса 
IAlertNotifier (и логер, и email), 
и автоматически подписываем их на уведомления при создании
AlertManager.
 */
// 9. Регистрируем MainService и ConsoleInterfaceService
builder.Services.AddSingleton<MainService>();
builder.Services.AddHostedService<ConsoleInterfaceService>();
//демонстрационный вариант 
builder.Services.AddHostedService<DemoScenarioService>();
// 10. Убираем прямую регистрацию MonitoringService как hosted service
// и регистрируем его как обычный сервис
builder.Services.AddSingleton<MonitoringService>();
// 11. Обновляем регистрацию DataProcessor
builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetRequiredService<ILogger<DataProcessor>>();
    return new DataProcessor(logger);
});






/*
 * Важный момент:
    Мы регистрируем DataProcessor как одиночный (Singleton),
потому что он один будет управлять стратегиями во всём приложении.
При его создании мы сразу устанавливаем стратегию по умолчанию
(ThresholdCheckStrategy).

 */


// 5. Собираем и запускаем хост
var host = builder.Build();

// Настраиваем обработку Ctrl+C
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    host.StopAsync().GetAwaiter().GetResult();
};
try
{
    var mainService = host.Services.GetRequiredService<MainService>();
    await mainService.StartAsync(CancellationToken.None);
    await host.RunAsync();
}
catch (OperationCanceledException)
{
    // Корректное завершение
}
catch (Exception ex)
{
    Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
}
finally
{
    host.Dispose();
}