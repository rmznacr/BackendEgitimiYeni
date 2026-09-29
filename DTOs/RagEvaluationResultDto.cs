namespace BackendEgitimiYeni.DTOs;

public class RagEvaluationResultDto
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public bool HasSources { get; set; }

    public bool KeywordsPassed { get; set; }

    public bool DocumentPassed { get; set; }

    public bool PagePassed { get; set; }

    public bool SimilarityPassed { get; set; }

    public bool Passed { get; set; }

    public double HighestSimilarity { get; set; }

    public List<string> MissingKeywords { get; set; } = new();

    public List<RagSourceDto> Sources { get; set; } = new();
}