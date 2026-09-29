using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IRagEvaluationService
{
    Task<RagEvaluationResultDto> EvaluateAsync(
        RagEvaluationRequestDto request,
        CancellationToken cancellationToken = default
    );
}