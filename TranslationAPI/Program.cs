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
logger.Info("���ε{���w�Ұ�");

var builder = WebApplication.CreateBuilder(args);

// �]�w NLog
builder.Logging.ClearProviders(); // �M����L��x���Ѫ�
builder.Host.UseNLog(); // �ϥ� NLog

// �t�m CORS�A��Chrome�X�R�\���קK���ШD�Q�T�����
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

// �[�J����A��
builder.Services.AddControllers();

// �t�mHttpClient�MGeminiTranslationService
builder.Services.AddHttpClient<IGeminiTranslationService, GeminiTranslationService>();

var app = builder.Build();
// �ϥ� CORS ����
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

