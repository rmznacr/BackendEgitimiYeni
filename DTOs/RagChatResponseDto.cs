namespace BackendEgitimiYeni.DTOs;

public class RagChatResponseDto
{
    public string ConversationId { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<RagSourceDto> Sources { get; set; } = new();
}