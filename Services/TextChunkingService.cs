using System.Text;
using System.Text.RegularExpressions;

namespace BackendEgitimiYeni.Services;

public class TextChunkingService : ITextChunkingService
{
    public List<string> CreateChunks(
        string text,
        int chunkSize = 1000,
        int chunkOverlap = 200)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        if (chunkSize <= 0)
        {
            throw new ArgumentException(
                "Chunk size 0'dan büyük olmalıdır."
            );
        }

        if (chunkOverlap < 0)
        {
            throw new ArgumentException(
                "Chunk overlap negatif olamaz."
            );
        }

        if (chunkOverlap >= chunkSize)
        {
            throw new ArgumentException(
                "Chunk overlap, chunk size değerinden küçük olmalıdır."
            );
        }

        var normalizedText =
            NormalizeText(text);

        var paragraphs =
            SplitIntoParagraphs(normalizedText);

        var chunks =
            new List<string>();

        var currentChunk =
            new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > chunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    AddChunk(
                        chunks,
                        currentChunk.ToString()
                    );

                    currentChunk.Clear();
                }

                var longParagraphChunks =
                    SplitLongParagraph(
                        paragraph,
                        chunkSize,
                        chunkOverlap
                    );

                chunks.AddRange(
                    longParagraphChunks
                );

                continue;
            }

            var additionalLength =
                currentChunk.Length == 0
                    ? paragraph.Length
                    : paragraph.Length + 2;

            if (
                currentChunk.Length +
                additionalLength <=
                chunkSize
            )
            {
                if (currentChunk.Length > 0)
                {
                    currentChunk.AppendLine();
                    currentChunk.AppendLine();
                }

                currentChunk.Append(paragraph);
            }
            else
            {
                var previousChunk =
                    currentChunk.ToString();

                AddChunk(
                    chunks,
                    previousChunk
                );

                currentChunk.Clear();

                var overlapText =
                    GetOverlapText(
                        previousChunk,
                        chunkOverlap
                    );

                if (!string.IsNullOrWhiteSpace(
                    overlapText))
                {
                    currentChunk.Append(
                        overlapText
                    );

                    currentChunk.AppendLine();
                    currentChunk.AppendLine();
                }

                currentChunk.Append(
                    paragraph
                );
            }
        }

        if (currentChunk.Length > 0)
        {
            AddChunk(
                chunks,
                currentChunk.ToString()
            );
        }

        return chunks;
    }

    private static string NormalizeText(
        string text)
    {
        var normalized =
            text.Replace(
                "\r\n",
                "\n"
            );

        normalized =
            normalized.Replace(
                "\r",
                "\n"
            );

        normalized =
            Regex.Replace(
                normalized,
                @"[ \t]+",
                " "
            );

        normalized =
            Regex.Replace(
                normalized,
                @"\n{3,}",
                "\n\n"
            );

        return normalized.Trim();
    }

    private static List<string> SplitIntoParagraphs(
        string text)
    {
        return text
            .Split(
                "\n\n",
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries
            )
            .Where(paragraph =>
                !string.IsNullOrWhiteSpace(
                    paragraph
                )
            )
            .ToList();
    }

    private static List<string> SplitLongParagraph(
        string paragraph,
        int chunkSize,
        int chunkOverlap)
    {
        var chunks =
            new List<string>();

        var sentences =
            Regex.Split(
                paragraph,
                @"(?<=[.!?])\s+"
            );

        var currentChunk =
            new StringBuilder();

        foreach (var sentence in sentences)
        {
            var cleanSentence =
                sentence.Trim();

            if (string.IsNullOrWhiteSpace(
                cleanSentence))
            {
                continue;
            }

            if (cleanSentence.Length > chunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    AddChunk(
                        chunks,
                        currentChunk.ToString()
                    );

                    currentChunk.Clear();
                }

                AddCharacterChunks(
                    chunks,
                    cleanSentence,
                    chunkSize,
                    chunkOverlap
                );

                continue;
            }

            var additionalLength =
                currentChunk.Length == 0
                    ? cleanSentence.Length
                    : cleanSentence.Length + 1;

            if (
                currentChunk.Length +
                additionalLength <=
                chunkSize
            )
            {
                if (currentChunk.Length > 0)
                {
                    currentChunk.Append(' ');
                }

                currentChunk.Append(
                    cleanSentence
                );
            }
            else
            {
                var previousChunk =
                    currentChunk.ToString();

                AddChunk(
                    chunks,
                    previousChunk
                );

                currentChunk.Clear();

                var overlapText =
                    GetOverlapText(
                        previousChunk,
                        chunkOverlap
                    );

                if (!string.IsNullOrWhiteSpace(
                    overlapText))
                {
                    currentChunk.Append(
                        overlapText
                    );

                    currentChunk.Append(' ');
                }

                currentChunk.Append(
                    cleanSentence
                );
            }
        }

        if (currentChunk.Length > 0)
        {
            AddChunk(
                chunks,
                currentChunk.ToString()
            );
        }

        return chunks;
    }

    private static void AddCharacterChunks(
        List<string> chunks,
        string text,
        int chunkSize,
        int chunkOverlap)
    {
        var startIndex = 0;

        while (startIndex < text.Length)
        {
            var length =
                Math.Min(
                    chunkSize,
                    text.Length - startIndex
                );

            var chunk =
                text.Substring(
                    startIndex,
                    length
                );

            AddChunk(
                chunks,
                chunk
            );

            if (
                startIndex + length >=
                text.Length
            )
            {
                break;
            }

            startIndex +=
                chunkSize - chunkOverlap;
        }
    }

    private static string GetOverlapText(
        string text,
        int overlapLength)
    {
        if (
            overlapLength <= 0 ||
            string.IsNullOrWhiteSpace(text)
        )
        {
            return string.Empty;
        }

        if (text.Length <= overlapLength)
        {
            return text.Trim();
        }

        var startIndex =
            text.Length - overlapLength;

        var overlap =
            text.Substring(startIndex);

        var firstSpace =
            overlap.IndexOf(' ');

        if (
            firstSpace >= 0 &&
            firstSpace <
            overlap.Length - 1
        )
        {
            overlap =
                overlap.Substring(
                    firstSpace + 1
                );
        }

        return overlap.Trim();
    }

    private static void AddChunk(
        List<string> chunks,
        string chunk)
    {
        var cleanChunk =
            chunk.Trim();

        if (!string.IsNullOrWhiteSpace(
            cleanChunk))
        {
            chunks.Add(cleanChunk);
        }
    }
}