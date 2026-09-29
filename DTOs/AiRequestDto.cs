using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class AiRequestDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;
}