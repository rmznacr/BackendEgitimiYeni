namespace BackendEgitimiYeni.DTOs;

public class DocumentResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ChunkCount { get; set; }
}