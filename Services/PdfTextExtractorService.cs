using BackendEgitimiYeni.Models;
using UglyToad.PdfPig;

namespace BackendEgitimiYeni.Services;

public class PdfTextExtractorService :
    IPdfTextExtractorService
{
    public Task<List<PdfPageContent>> ExtractPagesAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pages =
            new List<PdfPageContent>();

        using var pdfDocument =
            PdfDocument.Open(
                pdfStream
            );

        foreach (
            var page in
            pdfDocument.GetPages()
        )
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(
                page.Text))
            {
                continue;
            }

            pages.Add(
                new PdfPageContent
                {
                    PageNumber =
                        page.Number,

                    Content =
                        page.Text.Trim()
                }
            );
        }

        return Task.FromResult(
            pages
        );
    }
}