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
        public async Task<ActionResult<TranslationResponse>> Translate([FromBody] RootObject TextNodes)
        {
            if (TextNodes == null)
            {
                return BadRequest("HtmlContent and TextContent cannot be null or empty.");
            }
            var translatedText = await _translationService.TranslateToChineseAsync(TextNodes.TextNodes);

            return Ok(new TranslationResponse
            {
                TranslatedTextContent = translatedText
            });
        }

    }
}
