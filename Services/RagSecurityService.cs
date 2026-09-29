using System.Text.RegularExpressions;

namespace BackendEgitimiYeni.Services;

public class RagSecurityService :
    IRagSecurityService
{
    private static readonly string[] SuspiciousPatterns =
    {
        "ignore previous instructions",
        "ignore all previous instructions",
        "forget previous instructions",
        "disregard previous instructions",
        "system prompt",
        "developer message",
        "önceki talimatları unut",
        "önceki talimatları görmezden gel",
        "tüm talimatları unut",
        "sistem promptunu",
        "sistem mesajını"
    };

    public bool ContainsSuspiciousInstructions(
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        return SuspiciousPatterns.Any(
            pattern =>
                content.Contains(
                    pattern,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }

    public string SanitizeDocumentContent(
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var sanitized =
            content.Trim();

        sanitized =
            Regex.Replace(
                sanitized,
                @"<\s*(system|assistant|developer)\s*>",
                string.Empty,
                RegexOptions.IgnoreCase
            );

        sanitized =
            Regex.Replace(
                sanitized,
                @"<\s*/\s*(system|assistant|developer)\s*>",
                string.Empty,
                RegexOptions.IgnoreCase
            );

        return sanitized;
    }
}