namespace TranslationAPI.Model
{
    public class TranslationContent
    {

    }

    public class HtmlContentRequest
    {
        public string HtmlContent { get; set; }
        public string TextContent { get; set; }
    }

    public class TranslationResponse
    {
        public string TranslatedHtmlContent { get; set; }
    }

    public class GeminiApiResponse
    {
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        public Content Content { get; set; }
    }

    public class Content
    {
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
    }
}
