using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BackendEgitimiYeni.DTOs;

public class DocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Range(100, 10000)]
    public int ChunkSize { get; set; } = 1000;

    [Range(0, 5000)]
    public int ChunkOverlap { get; set; } = 200;
}