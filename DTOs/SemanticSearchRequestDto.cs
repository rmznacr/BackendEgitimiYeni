using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class SemanticSearchRequestDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(2000)]
    public string Query { get; set; } = string.Empty;
}