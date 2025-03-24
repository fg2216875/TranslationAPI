using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TranslationAPI.Interface;
using TranslationAPI.Model;
using WebApplication2.Service;
using Microsoft.Extensions.Logging;
using NLog;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication2.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGeminiTranslationService _translationService;

        public TranslationController(IGeminiTranslationService translationService)
        {
            _translationService = translationService;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        [Route("translate")]
        public async Task<ActionResult<TranslationResponse>> Translate([FromBody] HtmlContentRequest TextNodes)
        {
            var stopwatch = Stopwatch.StartNew(); // 開始計時
            try
            {
                if (TextNodes == null)
                {
                    return BadRequest("HtmlContent and TextContent cannot be null or empty.");
                }

                List<string[]> engTextResult = _translationService.SplitText(TextNodes);

                //var tasks = engTextResult.Select(engTexts => _translationService.TranslateToChineseLocalAsync(engTexts)).ToList();
                //await Task.WhenAll(tasks);
                var tasks = engTextResult.Select(engTexts => _translationService.TranslateToChineseAsync(engTexts,TextNodes.translateType)).ToList();
                await Task.WhenAll(tasks);

                var translatedText = _translationService.ConvertToDictionary(tasks);

                return Ok(new TranslationResponse
                {
                    TranslatedTextContent = translatedText
                });
            }
            catch (Exception ex) {
                logger.Error(ex.ToString());
                return StatusCode(500, new { error = "翻譯過程出現錯誤" });
            }
            finally
            {
                stopwatch.Stop();
                logger.Info($" api/translate 使用模型: {TextNodes.translateType} 執行時間: {stopwatch.Elapsed.TotalSeconds:F2} s");
            }
        }

    }
}
