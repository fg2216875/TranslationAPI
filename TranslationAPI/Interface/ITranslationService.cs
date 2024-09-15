using System.Collections.Concurrent;
using TranslationAPI.Model;

namespace TranslationAPI.Interface
{
    public interface ITranslationService
    {
        
    }

    public interface IGeminiTranslationService
    {
        Task<string> TranslateToChineseAsync(string[] textArray);
        Task<Dictionary<string,string>> TranslateToChineseAsync(TextNodes textNodes);

        //public Dictionary<string, string> SetTranslationText(string originalText, string translatedText);
        Dictionary<string, string> ConvertToDictionary(string inputText);
    }
}
