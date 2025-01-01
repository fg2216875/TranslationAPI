using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using TranslationAPI.Interface;
using WebApplication2.Service;
using LogLevel = NLog.LogLevel;

NLogConfig.ConfigureLogging();
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);
// 配置 CORS，讓Chrome擴充功能避免跨域請求被禁止的情形
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChromeExtension",
        builder =>
        {
            builder.WithOrigins("chrome-extension://jkmagcoibmeokinnnbcjkipginjeeahp")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
// Add services to the container.
// 加入控制器服務
builder.Services.AddControllers();

// 配置HttpClient和GeminiTranslationService
builder.Services.AddHttpClient<IGeminiTranslationService, GeminiTranslationService>();

// 設定 NLog
builder.Logging.ClearProviders(); // 清除其他日誌提供者
builder.Host.UseNLog(); // 使用 NLog

var app = builder.Build();
// 使用 CORS 策略
app.UseCors("AllowChromeExtension");
app.MapControllers();
app.UseAuthorization();
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

logger.Info("應用程式已啟動");
app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class NLogConfig
{
    public static void ConfigureLogging()
    {
        var config = new LoggingConfiguration();

        // 檢查專案根目錄下是否存在 "Log" 資料夾，如果不存在則建立
        string logDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Log"));
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=tostring}"
        };
        config.AddTarget(consoleTarget);

        var fileTarget = new FileTarget("file")
        {
            FileName = Path.Combine(logDirectory, "myapp-log-${shortdate}.log"),
            Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=tostring}"
        };
        config.AddTarget(fileTarget);

        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
        config.AddRule(LogLevel.Info, LogLevel.Warn, fileTarget);

        LogManager.Configuration = config;
    }
}

