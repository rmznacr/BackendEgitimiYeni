using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/embeddings")]
[Authorize]
public class EmbeddingsController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public EmbeddingsController(
        IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpPost]
    public async Task<ActionResult<EmbeddingResponseDto>>
        CreateEmbedding(
            EmbeddingRequestDto dto,
            CancellationToken cancellationToken)
    {
        var result =
            await _embeddingService.CreateEmbeddingAsync(
                dto.Text,
                cancellationToken
            );

        return Ok(result);
    }
}