using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public interface IQueryRewritingService
{
    Task<string> RewriteAsync(
        string currentMessage,
        IReadOnlyCollection<ChatMessage> history,
        CancellationToken cancellationToken = default
    );
}