namespace BackendEgitimiYeni.Services;

public interface ITextChunkingService
{
    List<string> CreateChunks(
        string text,
        int chunkSize = 1000,
        int chunkOverlap = 200
    );
}