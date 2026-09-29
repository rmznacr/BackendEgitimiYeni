using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IRagService
{
    Task<RagResponseDto> AskAsync(
        string question,
        CancellationToken cancellationToken = default
    );

    Task<RagChatResponseDto> ChatAsync(
        RagChatRequestDto request,
        CancellationToken cancellationToken = default
    );
}