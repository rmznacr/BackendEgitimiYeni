using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public class QueryRewritingService
    : IQueryRewritingService
{
    private readonly IAiService _aiService;
    private readonly ILogger<QueryRewritingService> _logger;

    public QueryRewritingService(
        IAiService aiService,
        ILogger<QueryRewritingService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<string> RewriteAsync(
        string currentMessage,
        IReadOnlyCollection<ChatMessage> history,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentMessage))
        {
            return currentMessage;
        }

        if (history.Count == 0)
        {
            return currentMessage;
        }

        var historyText =
            string.Join(
                "\n",
                history.Select(
                    message =>
                        $"{message.Role}: {message.Content}"
                )
            );

        var prompt =
            $"""
            GÖREV:

            Kullanıcının son mesajını,
            doküman araması için bağımsız ve açık
            bir arama sorgusuna dönüştür.

            KURALLAR:

            - Önceki konuşmayı yalnızca bağlamı
              anlamak için kullan.
            - Kullanıcının asıl niyetini değiştirme.
            - Yeni bilgi ekleme.
            - Soruyu cevaplama.
            - Açıklama yapma.
            - Yalnızca yeniden yazılmış sorguyu döndür.
            - Sorguyu Türkçe oluştur.
            - "bu", "bunun", "o", "onun",
              "ikinci yöntem" gibi belirsiz ifadeleri
              mümkünse konuşma geçmişinden açık hale getir.
            - Sorguyu kısa ve arama için uygun tut.

            <conversation_history>
            {historyText}
            </conversation_history>

            <current_message>
            {currentMessage}
            </current_message>
            """;

        try
        {
            var response =
                await _aiService.AskAsync(
                    prompt,
                    cancellationToken
                );

            var rewrittenQuery =
                response.Response?.Trim();

            if (string.IsNullOrWhiteSpace(rewrittenQuery))
            {
                return currentMessage;
            }

            _logger.LogInformation(
                "RAG query yeniden yazıldı. Original: {OriginalQuery}, Rewritten: {RewrittenQuery}",
                currentMessage,
                rewrittenQuery
            );

            return rewrittenQuery;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Query rewriting başarısız oldu. Orijinal sorgu kullanılacak."
            );

            return currentMessage;
        }
    }
}