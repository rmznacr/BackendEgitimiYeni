namespace BackendEgitimiYeni.Services;

public interface IRagSecurityService
{
    string SanitizeDocumentContent(
        string content
    );

    bool ContainsSuspiciousInstructions(
        string content
    );
}