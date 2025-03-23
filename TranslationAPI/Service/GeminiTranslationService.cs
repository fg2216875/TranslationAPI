using DeepL;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using TranslationAPI.Interface;
using TranslationAPI.Model;

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
            _httpClient.BaseAddress = new Uri(configuration["Gemini:URL"]);
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
        public async Task<string> TranslateToChineseAsync(string[] textArray, string translateType)
        {
            string englishText = string.Join("|", textArray);
            string systemInstruction = "你是一個翻譯助手，回答只能輸出翻譯後的內容";
            var prompt = $"'|'符號是字串中的分隔線，將分隔線內的各個'英文'翻譯成繁體中文(zh_TW)，要保留'|'符號: \"{englishText}\"";
            var request = new
            {
                contents = new[]
                {
                    new { parts = new[] { 
                        new { text = systemInstruction }, 
                        new { text = prompt } } }
                }
            };

            var model = string.Empty;
            if (translateType == "Gemini2.0")
            {
                model = $"models/gemini-2.0-flash:generateContent?key={_apiKey}";
            } else if (translateType == "Gemini2.0-lite") {
                model = $"models/gemini-2.0-flash-lite:generateContent?key={_apiKey}";
            } else {
                model = $"models/gemini-1.5-flash:generateContent?key={_apiKey}";
            }
                 
            var response = await _httpClient.PostAsJsonAsync(model,request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
            var chineseText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return chineseText;
        }  

        public async Task<string> TranslateToChineseLocalAsync(string[] textArray)
        {
            string englishText = string.Join("|", textArray);
            
            var prompt = $"將分隔線'|'內的各個'原文'翻譯成繁體中文(zh_TW)，並保留'|'符號: \"{englishText}\"";
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

        /// <summary>
        /// 將翻譯好的字串轉換成dictionary
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public Dictionary<string, string> ConvertToDictionary(string inputText)
        {
            string pattern = @"\{#(\d+)\}:\s*(.*?)(?=\||\z)";
            //避免字串中出現"空白"或"換行符"影響轉換效果
            inputText = inputText.Replace("\r\n", "").Replace("\n", "").Trim();
            var matches = Regex.Matches(inputText, pattern);

            return matches.Cast<Match>()
                .ToDictionary(
                    m => $"{{#{m.Groups[1].Value}}}",
                    m => m.Groups[2].Value.Trim()
                );
        }

        public List<string[]> SplitText(HtmlContentRequest TextNodes)
        {
            string englishText = "";
            foreach (var textNode in TextNodes.HTMLTextNodes.textNodes)
            {
                foreach (var kvp in textNode)
                {
                    // 處理每個節點的數據，合併成一個字串後再呼叫API進行翻譯
                    englishText += kvp.Key + ": " + kvp.Value + "|"; 
                }
            }
            // 將字串依據 '|' 符號分割
            string[] parts = englishText.TrimEnd('|').Split('|');
            // 計算每部分應該包含多少項目
            //var perPart = Math.Ceiling((decimal)parts.Length / 15);
            //List<string[]> engTextResult = new List<string[]>();
            //for (int i = 0; i < perPart; i++)
            //{
            //    string[] chunk = parts.Skip(i * 15).Take(15).ToArray();
            //    engTextResult.Add(chunk);
            //}

            //Gemini API免費版每分鐘可接收request次數為15次
            int requestCount = 8;
            //perPart 設定使用Gemini一次翻譯的數量
            int perPart = (int)Math.Ceiling(parts.Length / Convert.ToDecimal(requestCount));
            List<string[]> engTextResult = new List<string[]>();
            for (int i = 0; i < requestCount; i++)
            {
                string[] chunk = parts.Skip(i * perPart).Take(perPart).ToArray();
                engTextResult.Add(chunk);
            }

            return engTextResult;
        }
    } 
}
