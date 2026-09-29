namespace BackendEgitimiYeni.DTOs;

public class AiToolResponseDto
{
    public string Answer { get; set; } = string.Empty;

    public string? ToolName { get; set; }

    public bool ToolUsed { get; set; }
}