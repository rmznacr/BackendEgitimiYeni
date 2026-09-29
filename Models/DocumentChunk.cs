using Pgvector;

namespace BackendEgitimiYeni.Models;

public class DocumentChunk
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public int? PageNumber { get; set; }

    public Vector Embedding { get; set; } = null!;

    public Document Document { get; set; } = null!;
}