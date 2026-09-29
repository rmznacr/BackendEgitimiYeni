namespace BackendEgitimiYeni.Models;

public class Document
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public List<DocumentChunk> Chunks { get; set; } = new();
}