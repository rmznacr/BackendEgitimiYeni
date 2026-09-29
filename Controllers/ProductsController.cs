using Asp.Versioning;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(
        IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<
        ActionResult<PagedResultDto<ProductResponseDto>>>
        GetProducts(
            [FromQuery] ProductQueryDto queryDto)
    {
        if (queryDto.Page < 1)
        {
            return BadRequest(new
            {
                message =
                    "Page değeri en az 1 olmalıdır."
            });
        }

        if (queryDto.PageSize < 1 ||
            queryDto.PageSize > 100)
        {
            return BadRequest(new
            {
                message =
                    "PageSize değeri 1 ile 100 arasında olmalıdır."
            });
        }

        if (queryDto.MinPrice.HasValue &&
            queryDto.MinPrice.Value < 0)
        {
            return BadRequest(new
            {
                message =
                    "Minimum fiyat negatif olamaz."
            });
        }

        if (queryDto.MaxPrice.HasValue &&
            queryDto.MaxPrice.Value < 0)
        {
            return BadRequest(new
            {
                message =
                    "Maksimum fiyat negatif olamaz."
            });
        }

        if (queryDto.MinPrice.HasValue &&
            queryDto.MaxPrice.HasValue &&
            queryDto.MinPrice.Value >
            queryDto.MaxPrice.Value)
        {
            return BadRequest(new
            {
                message =
                    "Minimum fiyat maksimum fiyattan büyük olamaz."
            });
        }

        if (!string.IsNullOrWhiteSpace(queryDto.SortBy) &&
            queryDto.SortBy.ToLower() != "name" &&
            queryDto.SortBy.ToLower() != "price")
        {
            return BadRequest(new
            {
                message =
                    "SortBy yalnızca name veya price olabilir."
            });
        }

        if (!string.IsNullOrWhiteSpace(queryDto.SortOrder) &&
            queryDto.SortOrder.ToLower() != "asc" &&
            queryDto.SortOrder.ToLower() != "desc")
        {
            return BadRequest(new
            {
                message =
                    "SortOrder yalnızca asc veya desc olabilir."
            });
        }

        var products =
            await _productService.GetAllAsync(queryDto);

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>>
        GetProduct(int id)
    {
        var product =
            await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponseDto>>
        CreateProduct(ProductCreateDto dto)
    {
        var product =
            await _productService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetProduct),
            new
            {
                version = "1.0",
                id = product.Id
            },
            product
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        ProductUpdateDto dto)
    {
        var updated =
            await _productService.UpdateAsync(
                id,
                dto
            );

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        DeleteProduct(int id)
    {
        var deleted =
            await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}