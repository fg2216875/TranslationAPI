using System.Collections.Concurrent;
using TranslationAPI.Model;

namespace TranslationAPI.Interface
{
    public interface ITranslationService
    {
        
    }

    public interface IGeminiTranslationService
    {
        /// <summary>
        /// 使用Gemini API將英文翻譯成繁體中文
        /// </summary>
        /// <param name="textArray">英文句子</param>
        /// <returns></returns>
        Task<string> TranslateToChineseAsync(string[] textArray,string translateType);

        /// <summary>
        /// 使用local主機架設的語言模型來翻譯
        /// </summary>
        /// <param name="textArray"></param>
        /// <returns></returns>
        Task<string> TranslateToChineseLocalAsync(string[] textArray);

        /// <summary>
        /// 將接收到的文字進行切割並重組，整理成可給API翻譯的格式
        /// </summary>
        /// <param name="TextNodes">需要翻譯的文字節點</param>
        /// <returns></returns>
        List<string[]> SplitText(HtmlContentRequest TextNodes);
        //Task<Dictionary<string,string>> TranslateToChineseAsync(TextNodes textNodes);

        /// <summary>
        /// 把翻譯完成的字串轉換成Dictionary型別，是為了讓前端程式能夠用此
        /// </summary>
        /// <param name="inputText">翻譯好的字串</param>
        /// <returns></returns>
        Dictionary<string, string> ConvertToDictionary(List<Task<string>> inputText);
    }
}
