namespace TranslationAPI.Interface
{
    public interface ITranslationService
    {
        
    }

    public interface IGeminiTranslationService
    {
        Task<string> TranslateToChineseAsync(string englishText);

        public string ReplaceTextInHtml(string htmlContent, string originalText, string translatedText);

        public Dictionary<string, string> SetTranslationText(string originalText, string translatedText);
    }
}
