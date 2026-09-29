using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IEmbeddingService
{
    Task<EmbeddingResponseDto> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default
    );
}