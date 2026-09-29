using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductResponseDto>> GetAllAsync(
        ProductQueryDto queryDto
    );

    Task<ProductResponseDto?> GetByIdAsync(int id);

    Task<ProductResponseDto> CreateAsync(
        ProductCreateDto dto
    );

    Task<bool> UpdateAsync(
        int id,
        ProductUpdateDto dto
    );

    Task<bool> DeleteAsync(int id);
}