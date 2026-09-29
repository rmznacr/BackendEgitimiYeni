using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rag")]
[Authorize]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;

    public RagController(
        IRagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<RagResponseDto>> Ask(
        AiRequestDto dto,
        CancellationToken cancellationToken)
    {
        var response =
            await _ragService.AskAsync(
                dto.Prompt,
                cancellationToken
            );

        return Ok(response);
    }

    [HttpPost("chat")]
    public async Task<ActionResult<RagChatResponseDto>> Chat(
        RagChatRequestDto dto,
        CancellationToken cancellationToken)
    {
        var response =
            await _ragService.ChatAsync(
                dto,
                cancellationToken
            );

        return Ok(response);
    }
}