namespace BackendEgitimiYeni.DTOs;

public class EmbeddingResponseDto
{
    public int Dimensions { get; set; }

    public List<float> Embedding { get; set; } = new();
}