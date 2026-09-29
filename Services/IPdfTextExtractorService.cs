using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public interface IPdfTextExtractorService
{
    Task<List<PdfPageContent>> ExtractPagesAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default
    );
}