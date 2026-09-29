using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(
        IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AiResponseDto>> Ask(
        AiRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _aiService.AskAsync(
                dto.Prompt,
                cancellationToken
            );

        return Ok(result);
    }

    [HttpPost("explain")]
    public async Task<ActionResult<AiExplanationDto>> Explain(
        AiRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _aiService.ExplainAsync(
                dto.Prompt,
                cancellationToken
            );

        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponseDto>> Chat(
        ChatRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _aiService.ChatAsync(
                dto,
                cancellationToken
            );

        return Ok(result);
    }
}