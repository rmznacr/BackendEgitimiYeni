using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rag-evaluation")]
[Authorize]
public class RagEvaluationController : ControllerBase
{
    private readonly IRagEvaluationService
        _ragEvaluationService;

    public RagEvaluationController(
        IRagEvaluationService ragEvaluationService)
    {
        _ragEvaluationService =
            ragEvaluationService;
    }

    [HttpPost]
    public async Task<ActionResult<RagEvaluationResultDto>> Evaluate(
        RagEvaluationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result =
            await _ragEvaluationService
                .EvaluateAsync(
                    request,
                    cancellationToken
                );

        return Ok(result);
    }
}