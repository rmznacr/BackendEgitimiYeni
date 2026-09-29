using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class DocumentCreateDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Content { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    [Range(100, 10000)]
    public int ChunkSize { get; set; } = 1000;

    [Range(0, 5000)]
    public int ChunkOverlap { get; set; } = 200;
}