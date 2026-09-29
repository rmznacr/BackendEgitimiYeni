using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IAiToolService
{
    Task<AiToolResponseDto> ProcessAsync(
        AiToolRequestDto request,
        CancellationToken cancellationToken = default
    );
}