using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public class RagEvaluationService :
    IRagEvaluationService
{
    private readonly IRagService _ragService;

    private readonly ILogger<RagEvaluationService> _logger;

    public RagEvaluationService(
        IRagService ragService,
        ILogger<RagEvaluationService> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    public async Task<RagEvaluationResultDto> EvaluateAsync(
        RagEvaluationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ragResponse =
            await _ragService.AskAsync(
                request.Question,
                cancellationToken
            );

        var sources =
            ragResponse.Sources;

        var hasSources =
            sources.Count > 0;

        var missingKeywords =
            request.ExpectedKeywords
                .Where(keyword =>
                    !ragResponse.Answer.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();

        var keywordsPassed =
            missingKeywords.Count == 0;

        var documentPassed =
            string.IsNullOrWhiteSpace(
                request.ExpectedDocumentTitle
            )
            ||
            sources.Any(source =>
                string.Equals(
                    source.DocumentTitle,
                    request.ExpectedDocumentTitle,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                string.Equals(
                    source.FileName,
                    request.ExpectedDocumentTitle,
                    StringComparison.OrdinalIgnoreCase
                )
            );

        var pagePassed =
            request.ExpectedPageNumber is null
            ||
            sources.Any(source =>
                source.PageNumber ==
                request.ExpectedPageNumber
            );

        var highestSimilarity =
            sources.Count == 0
                ? 0
                : sources.Max(
                    source =>
                        source.Similarity
                );

        var similarityPassed =
            highestSimilarity >=
            request.MinimumSimilarity;

        var passed =
            hasSources
            &&
            keywordsPassed
            &&
            documentPassed
            &&
            pagePassed
            &&
            similarityPassed;

        _logger.LogInformation(
            "RAG evaluation tamamlandı. Question: {Question}, Passed: {Passed}, HighestSimilarity: {Similarity}",
            request.Question,
            passed,
            highestSimilarity
        );

        return new RagEvaluationResultDto
        {
            Question =
                request.Question,

            Answer =
                ragResponse.Answer,

            HasSources =
                hasSources,

            KeywordsPassed =
                keywordsPassed,

            DocumentPassed =
                documentPassed,

            PagePassed =
                pagePassed,

            SimilarityPassed =
                similarityPassed,

            Passed =
                passed,

            HighestSimilarity =
                highestSimilarity,

            MissingKeywords =
                missingKeywords,

            Sources =
                sources
        };
    }
}