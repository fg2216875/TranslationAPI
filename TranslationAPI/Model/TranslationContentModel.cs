namespace TranslationAPI.Model
{
    public class TextNode : Dictionary<string, string>
    {
        // 這個類繼承自Dictionary<string, string>，允許動態屬性名稱
    }

    public class TextNodes
    {
        public List<TextNode> textNodes { get; set; }
    }

    public class HtmlContentRequest
    {
        public TextNodes HTMLTextNodes { get; set; }
    }

    public class TranslationResponse
    {
        public Dictionary<string,string> TranslatedTextContent { get;set; }
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
