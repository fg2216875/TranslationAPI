using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using TranslationAPI.Interface;
using TranslationAPI.Model;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        /// <summary>
        /// 將英文原文翻譯成繁體中文
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        public async Task<Dictionary<string,string>> TranslateToChineseAsync(TextNodes textNodes)
        {
            string englishText = "";
            foreach (var textNode in textNodes.textNodes)
            {
                foreach (var kvp in textNode)
                {
                    // 處理每個節點的數據，合併成一個字串後再呼叫API進行翻譯
                    englishText += kvp.Key + ": " + kvp.Value + "|"; 
                }
            }

            var prompt = $"'|'符號是字串中的分隔線，將分隔線內的各個'英文'翻譯成繁體中文(zh_TW)，要保留'|'符號: \"{englishText}\"";
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
            var chineseDic = ConvertToDictionary(chineseText);

            return chineseDic;
        }  
        
        // 將翻譯後的文字替換到 HtmlContent 中
        public string ReplaceTextInHtml(string htmlContent, string originalText, string translatedText)
        {
            try
            {
                string result = htmlContent;
                var originalTextArray = originalText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                var translateTextArray = translatedText.Split("|", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < originalTextArray.Length; i++) { 
                    result = Regex.Replace(result, originalTextArray[i], translateTextArray[i]);
                }
 
                return result;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return error;
            }
        }

        public Dictionary<string,string> SetTranslationText(string originalText, string translatedText)
        {
            try
            {
                var originalTextArray = originalText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                var translateTextArray = translatedText.Split("|", StringSplitOptions.RemoveEmptyEntries);
                var dicTranslation = new Dictionary<string, string>();
                for(int i = 0; i < originalTextArray.Length; i++)
                {
                    if (dicTranslation.Keys.Contains(originalTextArray[i]))
                    {
                        continue;
                    }
                    dicTranslation.Add(originalTextArray[i], translateTextArray[i]);
                }

                return dicTranslation;
            }catch(Exception ex)
            {
                ex.ToString();
                return null;
            }
            
        }

        /// <summary>
        /// 將翻譯好的字串轉換成dictionary
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public Dictionary<string, string> ConvertToDictionary(string inputText)
        {
            string pattern = @"\{#(\d+)\}:\s*(.*?)(?=\||\z)";
            var matches = Regex.Matches(inputText, pattern);

            return matches.Cast<Match>()
                .ToDictionary(
                    m => $"{{#{m.Groups[1].Value}}}",
                    m => m.Groups[2].Value.Trim()
                );
        }
    } 
}
