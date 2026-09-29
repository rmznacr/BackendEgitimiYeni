namespace BackendEgitimiYeni.DTOs;

public class RagSourceDto
{
    public int DocumentId { get; set; }

    public int ChunkId { get; set; }

    public int ChunkIndex { get; set; }

    public string DocumentTitle { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public int? PageNumber { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Similarity { get; set; }
}