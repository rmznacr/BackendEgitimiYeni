using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IAiService
{
    Task<AiResponseDto> AskAsync(
        string prompt,
        CancellationToken cancellationToken = default
    );

    Task<AiExplanationDto> ExplainAsync(
        string prompt,
        CancellationToken cancellationToken = default
    );

    Task<ChatResponseDto> ChatAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<string> StreamAsync(
        string prompt,
        CancellationToken cancellationToken = default
    );
}