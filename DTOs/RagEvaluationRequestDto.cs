using System.ComponentModel.DataAnnotations;

namespace BackendEgitimiYeni.DTOs;

public class RagEvaluationRequestDto
{
    [Required]
    [MinLength(1)]
    public string Question { get; set; } = string.Empty;

    public List<string> ExpectedKeywords { get; set; } = new();

    public string? ExpectedDocumentTitle { get; set; }

    public int? ExpectedPageNumber { get; set; }

    [Range(0, 1)]
    public double MinimumSimilarity { get; set; } = 0.60;
}