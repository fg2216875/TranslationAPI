using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TranslationAPI.Interface;
using TranslationAPI.Model;
using WebApplication2.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication2.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        private readonly IGeminiTranslationService _translationService;

        public TranslationController(IGeminiTranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost]
        [Route("translate")]
        public async Task<ActionResult<TranslationResponse>> Translate([FromBody] HtmlContentRequest TextNodes)
        {
            if (TextNodes == null)
            {
                return BadRequest("HtmlContent and TextContent cannot be null or empty.");
            }
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
            string[] parts = englishText.Split('|');
            // 計算每部分應該包含多少項目
            //int itemsPerPart = (int)Math.Ceiling(parts.Length / 14.0);
            var perPart = Math.Ceiling((decimal)parts.Length / 10);
            // 將分割後的字串平均分成 9 個部分
            List<string[]> engTextResult = new List<string[]>();
            for (int i = 0; i < perPart; i++)
            {
                string[] chunk = parts.Skip(i * 10).Take(10).ToArray();
                engTextResult.Add(chunk);
            }

            var tasks = engTextResult.Select(engTexts =>  _translationService.TranslateToChineseAsync(engTexts)).ToList();
            await Task.WhenAll(tasks);

            string chineseText = "";
            for (int i = 0; i < tasks.Count; i++) {
                chineseText += tasks[i].Result.ToString() + "|";
            }
             var translatedText = _translationService.ConvertToDictionary(chineseText);

            return Ok(new TranslationResponse
            {
                TranslatedTextContent = translatedText
            });
        }

    }
}
