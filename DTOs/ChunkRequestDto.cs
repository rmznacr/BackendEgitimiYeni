using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class ChunkRequestDto
{
    [Required]
    public string Text { get; set; } = string.Empty;

    [Range(100, 10000)]
    public int ChunkSize { get; set; } = 1000;

    [Range(0, 5000)]
    public int ChunkOverlap { get; set; } = 200;
}