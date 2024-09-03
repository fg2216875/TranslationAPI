namespace TranslationAPI.Interface
{
    public interface ITranslationService
    {
        
    }

    public interface IGeminiTranslationService
    {
        Task<string> TranslateToChineseAsync(string englishText);
    }
}
