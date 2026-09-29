using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class RagChatRequestDto
{
    public string? ConversationId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}