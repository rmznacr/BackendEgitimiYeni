namespace BackendEgitimiYeni.DTOs;

public class AiExplanationDto
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = new();
}