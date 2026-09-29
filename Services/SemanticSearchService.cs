using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public class SemanticSearchService : ISemanticSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<SemanticSearchService> _logger;

    private static readonly List<string> Documents =
    [
        "ASP.NET Core kullanarak REST API ve Web API geliştirebilirsiniz.",

        "JWT Authentication kullanıcıların kimliğini doğrulamak için kullanılır.",

        "Entity Framework Core veritabanı işlemlerini kolaylaştıran bir ORM aracıdır.",

        "Dependency Injection sınıfların bağımlılıklarını dışarıdan almasını sağlar.",

        "Otomobil motorları yakıtın yanması sonucunda mekanik enerji üretir."
    ];

    public SemanticSearchService(
        IEmbeddingService embeddingService,
        ILogger<SemanticSearchService> logger)
    {
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<SemanticSearchResultDto> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding =
            await _embeddingService.CreateEmbeddingAsync(
                query,
                cancellationToken
            );

        string bestDocument = string.Empty;
        double bestSimilarity = double.MinValue;

        foreach (var document in Documents)
        {
            var documentEmbedding =
                await _embeddingService.CreateEmbeddingAsync(
                    document,
                    cancellationToken
                );

            var similarity =
                CosineSimilarity(
                    queryEmbedding.Embedding,
                    documentEmbedding.Embedding
                );

            _logger.LogInformation(
                "Document: {Document} - Similarity: {Similarity}",
                document,
                similarity
            );

            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                bestDocument = document;
            }
        }

        return new SemanticSearchResultDto
        {
            Text = bestDocument,
            Similarity = bestSimilarity
        };
    }

    private static double CosineSimilarity(
        IReadOnlyList<float> vectorA,
        IReadOnlyList<float> vectorB)
    {
        if (vectorA.Count != vectorB.Count)
        {
            throw new ArgumentException(
                "Embedding boyutları aynı olmalıdır."
            );
        }

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (var i = 0; i < vectorA.Count; i++)
        {
            dotProduct +=
                vectorA[i] * vectorB[i];

            magnitudeA +=
                vectorA[i] * vectorA[i];

            magnitudeB +=
                vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 ||
            magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct /
               (Math.Sqrt(magnitudeA) *
                Math.Sqrt(magnitudeB));
    }
}