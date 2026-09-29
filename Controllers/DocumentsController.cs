using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    private readonly IPdfTextExtractorService
        _pdfTextExtractorService;

    public DocumentsController(
        IDocumentService documentService,
        IPdfTextExtractorService pdfTextExtractorService)
    {
        _documentService = documentService;

        _pdfTextExtractorService =
            pdfTextExtractorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var documents =
            await _documentService.GetAllAsync(
                cancellationToken
            );

        return Ok(documents);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponseDto>> Create(
        DocumentCreateDto dto,
        CancellationToken cancellationToken)
    {
        var document =
            await _documentService.CreateAsync(
                dto,
                cancellationToken
            );

        return Ok(document);
    }

    [HttpPost("upload/txt")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentResponseDto>> UploadTxt(
        [FromForm] DocumentUploadDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.File.Length == 0)
        {
            return BadRequest(
                "Yüklenen dosya boş."
            );
        }

        const long maxFileSize =
            5 * 1024 * 1024;

        if (dto.File.Length > maxFileSize)
        {
            return BadRequest(
                "Dosya boyutu en fazla 5 MB olabilir."
            );
        }

        var extension =
            Path.GetExtension(
                dto.File.FileName
            );

        if (!string.Equals(
                extension,
                ".txt",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Bu endpoint yalnızca .txt dosyalarını kabul eder."
            );
        }

        if (dto.ChunkOverlap >= dto.ChunkSize)
        {
            return BadRequest(
                "ChunkOverlap, ChunkSize değerinden küçük olmalıdır."
            );
        }

        string content;

        using (
            var reader =
                new StreamReader(
                    dto.File.OpenReadStream()
                )
        )
        {
            content =
                await reader.ReadToEndAsync(
                    cancellationToken
                );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(
                "Dosyanın içerisinde metin bulunamadı."
            );
        }

        var createDto =
            new DocumentCreateDto
            {
                Title =
                    Path.GetFileNameWithoutExtension(
                        dto.File.FileName
                    ),

                Content = content,

                FileName =
                    dto.File.FileName,

                ContentType =
                    dto.File.ContentType,

                ChunkSize =
                    dto.ChunkSize,

                ChunkOverlap =
                    dto.ChunkOverlap
            };

        var document =
            await _documentService.CreateAsync(
                createDto,
                cancellationToken
            );

        return Ok(document);
    }

    [HttpPost("upload/pdf")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentResponseDto>> UploadPdf(
        [FromForm] DocumentUploadDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.File.Length == 0)
        {
            return BadRequest(
                "Yüklenen PDF dosyası boş."
            );
        }

        const long maxFileSize =
            10 * 1024 * 1024;

        if (dto.File.Length > maxFileSize)
        {
            return BadRequest(
                "PDF dosyası en fazla 10 MB olabilir."
            );
        }

        var extension =
            Path.GetExtension(
                dto.File.FileName
            );

        if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Bu endpoint yalnızca .pdf dosyalarını kabul eder."
            );
        }

        if (dto.ChunkOverlap >= dto.ChunkSize)
        {
            return BadRequest(
                "ChunkOverlap, ChunkSize değerinden küçük olmalıdır."
            );
        }

        List<BackendEgitimiYeni.Models.PdfPageContent>
            pages;

        using (
            var pdfStream =
                dto.File.OpenReadStream()
        )
        {
            pages =
                await _pdfTextExtractorService
                    .ExtractPagesAsync(
                        pdfStream,
                        cancellationToken
                    );
        }

        if (pages.Count == 0)
        {
            return BadRequest(
                "PDF içerisinden metin çıkarılamadı."
            );
        }

        var document =
            await _documentService
                .CreateFromPdfAsync(
                    Path.GetFileNameWithoutExtension(
                        dto.File.FileName
                    ),
                    dto.File.FileName,
                    dto.File.ContentType,
                    pages,
                    dto.ChunkSize,
                    dto.ChunkOverlap,
                    cancellationToken
                );

        return Ok(document);
    }
}