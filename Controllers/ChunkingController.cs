using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/chunking")]
[Authorize]
public class ChunkingController : ControllerBase
{
    private readonly ITextChunkingService _chunkingService;

    public ChunkingController(
        ITextChunkingService chunkingService)
    {
        _chunkingService = chunkingService;
    }

    [HttpPost]
    public ActionResult<ChunkResponseDto> CreateChunks(
        ChunkRequestDto dto)
    {
        var chunks =
            _chunkingService.CreateChunks(
                dto.Text,
                dto.ChunkSize,
                dto.ChunkOverlap
            );

        return Ok(
            new ChunkResponseDto
            {
                ChunkCount = chunks.Count,
                Chunks = chunks
            }
        );
    }
}