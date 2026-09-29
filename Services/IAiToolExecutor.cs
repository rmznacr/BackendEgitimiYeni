using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IAiToolExecutor
{
    Task<object> ExecuteGetProductsAsync(
        GetProductsToolArgumentsDto arguments,
        CancellationToken cancellationToken = default
    );
}