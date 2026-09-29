namespace BackendEgitimiYeni.DTOs;

public class ChunkResponseDto
{
    public int ChunkCount { get; set; }

    public List<string> Chunks { get; set; } = new();
}