using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface ISemanticSearchService
{
    Task<SemanticSearchResultDto> SearchAsync(
        string query,
        CancellationToken cancellationToken = default
    );
}