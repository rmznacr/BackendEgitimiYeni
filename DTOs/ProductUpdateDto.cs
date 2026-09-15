using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class ProductUpdateDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }
}