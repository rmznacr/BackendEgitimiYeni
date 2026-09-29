using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public interface IDocumentService
{
    Task<DocumentResponseDto> CreateAsync(
        DocumentCreateDto dto,
        CancellationToken cancellationToken = default
    );

    Task<DocumentResponseDto> CreateFromPdfAsync(
        string title,
        string fileName,
        string contentType,
        List<PdfPageContent> pages,
        int chunkSize,
        int chunkOverlap,
        CancellationToken cancellationToken = default
    );

    Task<List<DocumentResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
}