using BackendEgitimiYeni.Configuration;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace BackendEgitimiYeni.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaAiService> _logger;
    private readonly IMemoryCache _cache;
    private readonly AiSettings _aiSettings;

    private const string SystemPrompt =
        """
        Sen deneyimli bir backend yazılım eğitim asistanısın.

        Kurallar:
        - Her zaman Türkçe cevap ver.
        - Teknik kavramları anlaşılır şekilde açıkla.
        - Gereksiz uzun cevaplardan kaçın.
        - Bilmediğin bir konuda bilgi uydurma.
        - Kullanıcının sorusuna doğrudan cevap ver.
        """;

    public OllamaAiService(
    HttpClient httpClient,
    ILogger<OllamaAiService> logger,
    IMemoryCache cache,
    IOptions<AiSettings> aiOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _aiSettings = aiOptions.Value;
    }

    public async Task<AiResponseDto> AskAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _aiSettings.ChatModel,

            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = SystemPrompt
                },

                new
                {
                    role = "user",
                    content = prompt
                }
            },

            think = false,
            stream = false
        };

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Ollama normal isteği başlıyor."
        );

        var httpResponse =
            await _httpClient.PostAsJsonAsync(
                "/api/chat",
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

        var response =
            json.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
            ?? string.Empty;

        stopwatch.Stop();

        _logger.LogInformation(
            "Ollama normal cevabı {ElapsedSeconds} saniyede alındı.",
            stopwatch.Elapsed.TotalSeconds
        );

        return new AiResponseDto
        {
            Response = response
        };
    }

    public async Task<AiExplanationDto> ExplainAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        var prompt =
            $"""
            "{topic}" konusunu açıkla.

            Aşağıdaki alanları doldur:

            title:
            Konunun adı.

            summary:
            En fazla 2 cümlelik Türkçe açıklama.

            difficulty:
            beginner, intermediate veya advanced değerlerinden biri.

            keywords:
            Konuyla ilgili tam 3 anahtar kelime.
            """;

        var request = new
        {
            model = _aiSettings.ChatModel,

            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = SystemPrompt
                },

                new
                {
                    role = "user",
                    content = prompt
                }
            },

            format = "json",
            think = false,
            stream = false,

            options = new
            {
                temperature = 0.2,
                num_predict = 250
            }
        };

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Structured Output isteği başlıyor. Topic: {Topic}",
            topic
        );

        var httpResponse =
            await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken
            );

        _logger.LogInformation(
            "Structured Output HTTP cevabı {ElapsedSeconds} saniyede geldi.",
            stopwatch.Elapsed.TotalSeconds
        );

        httpResponse.EnsureSuccessStatusCode();

        using var json =
            await JsonDocument.ParseAsync(
                await httpResponse.Content.ReadAsStreamAsync(
                    cancellationToken
                ),
                cancellationToken: cancellationToken
            );

        var content =
            json.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "LLM boş cevap döndürdü."
            );
        }

        var result =
            JsonSerializer.Deserialize<AiExplanationDto>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

        if (result is null)
        {
            throw new InvalidOperationException(
                "LLM cevabı AiExplanationDto nesnesine dönüştürülemedi."
            );
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Structured Output işlemi {ElapsedSeconds} saniyede tamamlandı.",
            stopwatch.Elapsed.TotalSeconds
        );

        return result;
    }

    public async Task<ChatResponseDto> ChatAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var conversationId =
            string.IsNullOrWhiteSpace(request.ConversationId)
                ? Guid.NewGuid().ToString()
                : request.ConversationId;

        var cacheKey =
            $"ai-conversation-{conversationId}";

        var history =
            _cache.Get<List<ChatMessage>>(cacheKey)
            ?? new List<ChatMessage>();

        history.Add(
            new ChatMessage
            {
                Role = "user",
                Content = request.Message
            }
        );

        var messages =
            new List<object>
            {
                new
                {
                    role = "system",
                    content = SystemPrompt
                }
            };

        foreach (var message in history)
        {
            messages.Add(
                new
                {
                    role = message.Role,
                    content = message.Content
                }
            );
        }

        var ollamaRequest = new
        {
            model = _aiSettings.ChatModel,

            messages = messages,

            think = false,

            stream = false,

            options = new
            {
                temperature = 0.4,
                num_predict = 300
            }
        };

        _logger.LogInformation(
            "Chat isteği gönderiliyor. ConversationId: {ConversationId}, MessageCount: {MessageCount}",
            conversationId,
            history.Count
        );

        var httpResponse =
            await _httpClient.PostAsJsonAsync(
                "/api/chat",
                ollamaRequest,
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

        var response =
            json.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
            ?? string.Empty;

        history.Add(
            new ChatMessage
            {
                Role = "assistant",
                Content = response
            }
        );

        _cache.Set(
            cacheKey,
            history,
            TimeSpan.FromMinutes(30)
        );

        _logger.LogInformation(
            "Chat geçmişi kaydedildi. ConversationId: {ConversationId}",
            conversationId
        );

        return new ChatResponseDto
        {
            ConversationId = conversationId,
            Response = response
        };
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string prompt,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _aiSettings.ChatModel,

            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = SystemPrompt
                },

                new
                {
                    role = "user",
                    content = prompt
                }
            },

            think = false,

            stream = true,

            options = new
            {
                temperature = 0.4,
                num_predict = 500
            }
        };

        var jsonRequest =
            JsonSerializer.Serialize(
                request
            );

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/chat"
            );

        httpRequest.Content =
            new StringContent(
                jsonRequest,
                Encoding.UTF8,
                "application/json"
            );

        var stopwatch =
            Stopwatch.StartNew();

        _logger.LogInformation(
            "Ollama streaming isteği başlıyor."
        );

        using var httpResponse =
            await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

        httpResponse.EnsureSuccessStatusCode();

        await using var stream =
            await httpResponse.Content
                .ReadAsStreamAsync(
                    cancellationToken
                );

        using var reader =
            new StreamReader(
                stream
            );

        while (!cancellationToken.IsCancellationRequested)
        {
            var line =
                await reader.ReadLineAsync(
                    cancellationToken
                );

            if (line is null)
            {
                break;
            }
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var json =
                JsonDocument.Parse(
                    line
                );

            var root =
                json.RootElement;

            if (
                root.TryGetProperty(
                    "message",
                    out var message
                )
                &&
                message.TryGetProperty(
                    "content",
                    out var content
                )
            )
            {
                var text =
                    content.GetString();

                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }

            if (
                root.TryGetProperty(
                    "done",
                    out var done
                )
                &&
                done.GetBoolean()
            )
            {
                break;
            }
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Ollama streaming cevabı {ElapsedSeconds} saniyede tamamlandı.",
            stopwatch.Elapsed.TotalSeconds
        );
    }
}