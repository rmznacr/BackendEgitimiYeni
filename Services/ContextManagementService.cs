using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public class ContextManagementService :
    IContextManagementService
{
    private const int MaxContextCharacters = 6000;

    private const int MaxHistoryCharacters = 4000;

    private const int MaxHistoryMessages = 6;

    public List<RagSourceDto> LimitSources(
        List<RagSourceDto> sources)
    {
        var result =
            new List<RagSourceDto>();

        var currentLength = 0;

        foreach (
            var source in
            sources.OrderByDescending(
                source => source.Similarity
            )
        )
        {
            if (string.IsNullOrWhiteSpace(
                source.Content))
            {
                continue;
            }

            var remainingCharacters =
                MaxContextCharacters -
                currentLength;

            if (remainingCharacters <= 0)
            {
                break;
            }

            if (
                source.Content.Length <=
                remainingCharacters
            )
            {
                result.Add(
                    source
                );

                currentLength +=
                    source.Content.Length;

                continue;
            }

            var shortenedSource =
                new RagSourceDto
                {
                    DocumentId =
                        source.DocumentId,

                    ChunkId =
                        source.ChunkId,

                    ChunkIndex =
                        source.ChunkIndex,

                    DocumentTitle =
                        source.DocumentTitle,

                    FileName =
                        source.FileName,

                    PageNumber =
                        source.PageNumber,

                    Similarity =
                        source.Similarity,

                    Content =
                        source.Content.Substring(
                            0,
                            remainingCharacters
                        )
                };

            result.Add(
                shortenedSource
            );

            break;
        }

        return result;
    }

    public List<ChatMessage> LimitHistory(
        List<ChatMessage> history)
    {
        var result =
            new List<ChatMessage>();

        var currentLength = 0;

        foreach (
            var message in
            history
                .TakeLast(
                    MaxHistoryMessages
                )
                .Reverse()
        )
        {
            if (string.IsNullOrWhiteSpace(
                message.Content))
            {
                continue;
            }

            if (
                currentLength +
                message.Content.Length >
                MaxHistoryCharacters
            )
            {
                continue;
            }

            result.Add(
                new ChatMessage
                {
                    Role =
                        message.Role,

                    Content =
                        message.Content
                }
            );

            currentLength +=
                message.Content.Length;
        }

        result.Reverse();

        return result;
    }
}