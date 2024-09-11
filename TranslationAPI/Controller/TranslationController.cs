using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
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
            //string translatedHtmlContent = _translationService.ReplaceTextInHtml(request.HtmlContent, request.TextContent, translatedText);
            var dicTranslation = _translationService.SetTranslationText(request.TextContent, translatedText);

            //return Ok(new TranslationResponse
            //{
            //    TranslatedHtmlContent = translatedHtmlContent
            //});
            return Ok(new TranslationResponse
            {
                TranslatedTextContent = dicTranslation
            });
        }

    }
}
