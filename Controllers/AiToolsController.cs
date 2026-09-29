using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai-tools")]
[Authorize]
public class AiToolsController : ControllerBase
{
    private readonly IAiToolService
        _aiToolService;

    public AiToolsController(
        IAiToolService aiToolService)
    {
        _aiToolService =
            aiToolService;
    }

    [HttpPost]
    public async Task<ActionResult<AiToolResponseDto>> Process(
        AiToolRequestDto request,
        CancellationToken cancellationToken)
    {
        var response =
            await _aiToolService.ProcessAsync(
                request,
                cancellationToken
            );

        return Ok(response);
    }
}