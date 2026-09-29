using BackendEgitimiYeni.Configuration;
using BackendEgitimiYeni.DTOs;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace BackendEgitimiYeni.Services;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaEmbeddingService> _logger;
    private readonly AiSettings _aiSettings;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        ILogger<OllamaEmbeddingService> logger,
        IOptions<AiSettings> aiOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _aiSettings = aiOptions.Value;
    }

    public async Task<EmbeddingResponseDto> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _aiSettings.EmbeddingModel,
            input = text
        };

        _logger.LogInformation(
            "Embedding oluşturuluyor. Model: {Model}",
            _aiSettings.EmbeddingModel
        );

        var httpResponse =
            await _httpClient.PostAsJsonAsync(
                "/api/embed",
                request,
                cancellationToken
            );

        httpResponse.EnsureSuccessStatusCode();

        using var json =
            await JsonDocument.ParseAsync(
                await httpResponse.Content.ReadAsStreamAsync(
                    cancellationToken
                ),
                cancellationToken: cancellationToken
            );

        var embeddingsElement =
            json.RootElement.GetProperty("embeddings");

        if (embeddingsElement.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "Embedding modeli boş sonuç döndürdü."
            );
        }

        var firstEmbedding =
            embeddingsElement[0];

        var embedding = new List<float>();

        foreach (var value in firstEmbedding.EnumerateArray())
        {
            embedding.Add(
                value.GetSingle()
            );
        }

        _logger.LogInformation(
            "Embedding oluşturuldu. Model: {Model}, Dimensions: {Dimensions}",
            _aiSettings.EmbeddingModel,
            embedding.Count
        );

        return new EmbeddingResponseDto
        {
            Dimensions = embedding.Count,
            Embedding = embedding
        };
    }
}