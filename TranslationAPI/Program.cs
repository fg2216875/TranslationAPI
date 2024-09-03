using TranslationAPI.Interface;
using WebApplication2.Service;

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
