namespace BackendEgitimiYeni.Configuration;

public class AiSettings
{
    public const string SectionName = "AI";

    public string BaseUrl { get; set; } =
        "http://localhost:11434";

    public string ChatModel { get; set; } =
        "qwen3:1.7b";

    public string EmbeddingModel { get; set; } =
        "nomic-embed-text";

    public int TimeoutSeconds { get; set; } = 120;
}