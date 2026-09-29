namespace BackendEgitimiYeni.DTOs;

public class SemanticSearchResultDto
{
    public string Text { get; set; } = string.Empty;

    public double Similarity { get; set; }
}