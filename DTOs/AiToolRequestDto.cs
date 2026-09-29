using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class AiToolRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}