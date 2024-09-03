using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using TranslationAPI.Interface;
using TranslationAPI.Model;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication2.Service
{
    public class GeminiTranslationService : IGeminiTranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiTranslationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }

        public async Task<string> TranslateToChineseAsync(string englishText)
        {
            // 去除TextContent中的換行符號
            englishText = englishText.Replace("\n", "|");
            //var prompt = $"Translate the following English text to Chinese(zh_TW): \"{englishText}\"";
            var prompt = $"將這個字串中的'英文'翻譯成繁體中文(zh_TW)，要保留'|'符號: \"{englishText}\"";
            var request = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"models/gemini-pro:generateContent?key={_apiKey}",
                request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
            var chineseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            return chineseText;
        }        
    }
    //public interface IGeminiTranslationService
    //{
    //    Task<string> TranslateToChineseAsync(string englishText);
    //}
    
}
