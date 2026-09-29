using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace BackendEgitimiYeni.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _dbContext;

    private readonly IEmbeddingService _embeddingService;

    private readonly ITextChunkingService _chunkingService;

    public DocumentService(
        AppDbContext dbContext,
        IEmbeddingService embeddingService,
        ITextChunkingService chunkingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _chunkingService = chunkingService;
    }

    public async Task<DocumentResponseDto> CreateAsync(
        DocumentCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var chunks =
            _chunkingService.CreateChunks(
                dto.Content,
                dto.ChunkSize,
                dto.ChunkOverlap
            );

        var document =
            new Document
            {
                Title = dto.Title,
                Content = dto.Content,
                FileName = dto.FileName,
                ContentType = dto.ContentType,
                CreatedAt = DateTime.UtcNow
            };

        for (var i = 0; i < chunks.Count; i++)
        {
            var documentChunk =
                await CreateChunkAsync(
                    chunks[i],
                    i,
                    null,
                    cancellationToken
                );

            document.Chunks.Add(
                documentChunk
            );
        }

        _dbContext.Documents.Add(
            document
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return MapDocument(
            document
        );
    }

    public async Task<DocumentResponseDto> CreateFromPdfAsync(
        string title,
        string fileName,
        string contentType,
        List<PdfPageContent> pages,
        int chunkSize,
        int chunkOverlap,
        CancellationToken cancellationToken = default)
    {
        var document =
            new Document
            {
                Title = title,
                FileName = fileName,
                ContentType = contentType,
                CreatedAt = DateTime.UtcNow,
                Content = string.Join(
                    "\n\n",
                    pages.Select(
                        page => page.Content
                    )
                )
            };

        var chunkIndex = 0;

        foreach (var page in pages)
        {
            var pageChunks =
                _chunkingService.CreateChunks(
                    page.Content,
                    chunkSize,
                    chunkOverlap
                );

            foreach (var content in pageChunks)
            {
                var documentChunk =
                    await CreateChunkAsync(
                        content,
                        chunkIndex,
                        page.PageNumber,
                        cancellationToken
                    );

                document.Chunks.Add(
                    documentChunk
                );

                chunkIndex++;
            }
        }

        _dbContext.Documents.Add(
            document
        );

        await _dbContext.SaveChangesAsync(
            cancellationToken
        );

        return MapDocument(
            document
        );
    }

    public async Task<List<DocumentResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .AsNoTracking()
            .Select(document =>
                new DocumentResponseDto
                {
                    Id = document.Id,
                    Title = document.Title,
                    Content = document.Content,
                    FileName = document.FileName,
                    ContentType =
                        document.ContentType,
                    CreatedAt =
                        document.CreatedAt,
                    ChunkCount =
                        document.Chunks.Count
                }
            )
            .ToListAsync(cancellationToken);
    }

    private async Task<DocumentChunk> CreateChunkAsync(
        string content,
        int chunkIndex,
        int? pageNumber,
        CancellationToken cancellationToken)
    {
        var embeddingResult =
            await _embeddingService
                .CreateEmbeddingAsync(
                    content,
                    cancellationToken
                );

        return new DocumentChunk
        {
            Content = content,

            ChunkIndex = chunkIndex,

            PageNumber = pageNumber,

            Embedding =
                new Vector(
                    embeddingResult
                        .Embedding
                        .ToArray()
                )
        };
    }

    private static DocumentResponseDto MapDocument(
        Document document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            FileName = document.FileName,
            ContentType = document.ContentType,
            CreatedAt = document.CreatedAt,
            ChunkCount =
                document.Chunks.Count
        };
    }
}