using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class EmbeddingRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Text { get; set; } = string.Empty;
}