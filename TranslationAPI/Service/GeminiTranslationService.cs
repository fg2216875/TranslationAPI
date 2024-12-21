using DeepL;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }

        /// <summary>
        /// 將英文原文翻譯成繁體中文
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        //public async Task<Dictionary<string,string>> TranslateToChineseAsync(TextNodes textNodes)
        //{
        //    string englishText = "";
        //    foreach (var textNode in textNodes.textNodes)
        //    {
        //        foreach (var kvp in textNode)
        //        {
        //            // 處理每個節點的數據，合併成一個字串後再呼叫API進行翻譯
        //            englishText += kvp.Key + ": " + kvp.Value + "|"; 
        //        }
        //    }

        //    var prompt = $"'|'符號是字串中的分隔線，將分隔線內的各個'英文'翻譯成繁體中文(zh_TW)，要保留'|'符號: \"{englishText}\"";
        //    var request = new
        //    {
        //        contents = new[]
        //        {
        //            new { parts = new[] { new { text = prompt } } }
        //        }
        //    };

        //    var response = await _httpClient.PostAsJsonAsync(
        //        $"models/gemini-pro:generateContent?key={_apiKey}",
        //        request);
        //    response.EnsureSuccessStatusCode();

        //    var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
        //    var chineseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        //    var chineseDic = ConvertToDictionary(chineseText);

        //    return chineseDic;
        //}  
        
        /// <summary>
        /// 使用Gemini API將英文翻譯成繁體中文
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        public async Task<string> TranslateToChineseAsync(string[] textArray)
        {
            string englishText = string.Join("|", textArray);
            //var prompt = $"The '|' symbol is the delimiter in the string. Translate each 'English' word between the delimiters into Traditional Chinese (zh_TW), while keeping the '|' symbol: \"{englishText}\"";
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
            //var chineseDic = ConvertToDictionary(chineseText);

            return chineseText;
        }  

        public async Task<string> TranslateToChineseLocalAsync(string[] textArray)
        {
            string englishText = string.Join("|", textArray);
            var prompt = $"將分隔線'|'內的各個'英文'翻譯成繁體中文(zh_TW)，要保留'|'符號: \"{englishText}\"";
            var request = new
            {
                model = "LM Studio Community/Meta-Llama-3-8B-Instruct-GGUF",
                messages = new[] {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = -1,
                stream = false
            };
    
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:1234/v1/chat/completions",
                request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LocalAPIResponse>();
            var chineseText = result?.Choices[0].Message.Content;

            return chineseText;
        }

        //public Dictionary<string,string> SetTranslationText(string originalText, string translatedText)
        //{
        //    try
        //    {
        //        var originalTextArray = originalText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        //        var translateTextArray = translatedText.Split("|", StringSplitOptions.RemoveEmptyEntries);
        //        var dicTranslation = new Dictionary<string, string>();
        //        for(int i = 0; i < originalTextArray.Length; i++)
        //        {
        //            if (dicTranslation.Keys.Contains(originalTextArray[i]))
        //            {
        //                continue;
        //            }
        //            dicTranslation.Add(originalTextArray[i], translateTextArray[i]);
        //        }

        //        return dicTranslation;
        //    }catch(Exception ex)
        //    {
        //        ex.ToString();
        //        return null;
        //    }
            
        //}

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
