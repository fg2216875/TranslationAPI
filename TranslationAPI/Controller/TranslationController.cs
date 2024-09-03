using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> TranslateHtml([FromBody] HtmlContentRequest request)
        {
            var chineseTranslation = await _translationService.TranslateToChineseAsync(request.HtmlContent);
            return Ok(new { Translation = chineseTranslation });
        }

        [HttpPost]
        [Route("translate")]
        public async Task<ActionResult<TranslationResponse>> Translate([FromBody] HtmlContentRequest request)
        {
            if (string.IsNullOrEmpty(request.HtmlContent) || string.IsNullOrEmpty(request.TextContent))
            {
                return BadRequest("HtmlContent and TextContent cannot be null or empty.");
            }
            string translatedText = await _translationService.TranslateToChineseAsync(request.TextContent);
            // 將翻譯後的文字替換到 HtmlContent 中
            string translatedHtmlContent = ReplaceTextInHtml(request.HtmlContent, request.TextContent, translatedText);

            return Ok(new TranslationResponse
            {
                TranslatedHtmlContent = translatedHtmlContent
            });
        }

        // 將翻譯後的文字替換到 HtmlContent 中
        private string ReplaceTextInHtml(string htmlContent, string originalText, string translatedText)
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

        //// 模擬翻譯方法 (將英文翻譯成中文)
        //private string TranslateText(string text)
        //{
        //    // 假設翻譯邏輯，實際應調用翻譯API
        //    return text.Replace("Hello", "你好").Replace("World", "世界");
        //}

        //public class HtmlContentRequest
        //{
        //    public string HtmlContent { get; set; }
        //    public string TextContent { get; set; }
        //}

        //public class TranslationResponse
        //{
        //    public string TranslatedHtmlContent { get; set; }
        //}
    }
}
