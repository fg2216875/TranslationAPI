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
// Add services to the container.
// �[�J����A��
builder.Services.AddControllers();

// �t�mHttpClient�MGeminiTranslationService
builder.Services.AddHttpClient<IGeminiTranslationService, GeminiTranslationService>();

// �]�w NLog
builder.Logging.ClearProviders(); // �M����L��x���Ѫ�
builder.Host.UseNLog(); // �ϥ� NLog

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

logger.Info("���ε{���w�Ұ�");
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

        // �ˬd�M�׮ڥؿ��U�O�_�s�b "Log" ��Ƨ��A�p�G���s�b�h�إ�
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

