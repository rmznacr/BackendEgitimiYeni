using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai-stream")]
[Authorize]
public class AiStreamingController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiStreamingController(
        IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task Stream(
        AiRequestDto request,
        CancellationToken cancellationToken)
    {
        Response.ContentType =
            "text/plain; charset=utf-8";

        Response.Headers.CacheControl =
            "no-cache";

        await foreach (
            var chunk in
            _aiService.StreamAsync(
                request.Prompt,
                cancellationToken
            )
        )
        {
            await Response.WriteAsync(
                chunk,
                cancellationToken
            );

            await Response.Body.FlushAsync(
                cancellationToken
            );
        }
    }
}