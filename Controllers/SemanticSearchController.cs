using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/semantic-search")]
[Authorize]
public class SemanticSearchController : ControllerBase
{
    private readonly ISemanticSearchService
        _semanticSearchService;

    public SemanticSearchController(
        ISemanticSearchService semanticSearchService)
    {
        _semanticSearchService =
            semanticSearchService;
    }

    [HttpPost]
    public async Task<
        ActionResult<SemanticSearchResultDto>>
        Search(
            SemanticSearchRequestDto dto,
            CancellationToken cancellationToken)
    {
        var result =
            await _semanticSearchService.SearchAsync(
                dto.Query,
                cancellationToken
            );

        return Ok(result);
    }
}