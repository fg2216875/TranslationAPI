using TranslationAPI.Model;

namespace TranslationAPI.Interface
{
    public interface ITranslationService
    {
        
    }

    public interface IGeminiTranslationService
    {
        Task<string> TranslateToChineseAsync(string englishText);
        Task<Dictionary<string,string>> TranslateToChineseAsync(TextNodes textNodes);

        public string ReplaceTextInHtml(string htmlContent, string originalText, string translatedText);

        public Dictionary<string, string> SetTranslationText(string originalText, string translatedText);
        Dictionary<string, string> ConvertToDictionary(string inputText);
    }
}
