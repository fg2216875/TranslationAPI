using Microsoft.AspNetCore.HttpLogging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Web;
using TranslationAPI.Interface;
using WebApplication2.Service;
using LogLevel = NLog.LogLevel;

var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
logger.Info("應用程式已啟動");

var builder = WebApplication.CreateBuilder(args);

// 設定 NLog
builder.Logging.ClearProviders(); // 清除其他日誌提供者
builder.Host.UseNLog(); // 使用 NLog

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

// 加入控制器服務
builder.Services.AddControllers();

// 配置HttpClient和GeminiTranslationService
builder.Services.AddHttpClient<IGeminiTranslationService, GeminiTranslationService>();

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
app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

