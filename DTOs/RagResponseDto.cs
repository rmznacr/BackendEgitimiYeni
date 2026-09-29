namespace BackendEgitimiYeni.DTOs;

public class RagResponseDto
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<RagSourceDto> Sources { get; set; } = new();
}