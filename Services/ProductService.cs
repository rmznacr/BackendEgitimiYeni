using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace BackendEgitimiYeni.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _cache;

    private static CancellationTokenSource
        _productCacheTokenSource = new();

    public ProductService(
        AppDbContext context,
        ILogger<ProductService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<PagedResultDto<ProductResponseDto>> GetAllAsync(
        ProductQueryDto queryDto)
    {
        var cacheKey =
            $"products_" +
            $"{queryDto.Page}_" +
            $"{queryDto.PageSize}_" +
            $"{queryDto.Search}_" +
            $"{queryDto.MinPrice}_" +
            $"{queryDto.MaxPrice}_" +
            $"{queryDto.SortBy}_" +
            $"{queryDto.SortOrder}";

        if (_cache.TryGetValue(
            cacheKey,
            out PagedResultDto<ProductResponseDto>? cachedResult))
        {
            _logger.LogInformation(
                "Ürünler cache üzerinden getirildi. CacheKey: {CacheKey}",
                cacheKey
            );

            return cachedResult!;
        }

        _logger.LogInformation(
            "Cache bulunamadı. Ürünler veritabanından getiriliyor."
        );

        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var search = queryDto.Search.ToLower();

            query = query.Where(
                p => p.Name.ToLower().Contains(search)
            );
        }

        if (queryDto.MinPrice.HasValue)
        {
            query = query.Where(
                p => p.Price >= queryDto.MinPrice.Value
            );
        }

        if (queryDto.MaxPrice.HasValue)
        {
            query = query.Where(
                p => p.Price <= queryDto.MaxPrice.Value
            );
        }

        var totalCount =
            await query.CountAsync();

        query = queryDto.SortBy?.ToLower() switch
        {
            "name" =>
                queryDto.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

            "price" =>
                queryDto.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

            _ => query.OrderBy(p => p.Id)
        };

        var products = await query
            .Skip(
                (queryDto.Page - 1)
                * queryDto.PageSize
            )
            .Take(queryDto.PageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalCount
                / (double)queryDto.PageSize
            );

        var result =
            new PagedResultDto<ProductResponseDto>
            {
                Items = products,
                Page = queryDto.Page,
                PageSize = queryDto.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

        var cacheOptions =
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(5)
                )
                .AddExpirationToken(
                    new CancellationChangeToken(
                        _productCacheTokenSource.Token
                    )
                );

        _cache.Set(
            cacheKey,
            result,
            cacheOptions
        );

        _logger.LogInformation(
            "Ürünler cache'e kaydedildi. CacheKey: {CacheKey}",
            cacheKey
        );

        return result;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .FirstOrDefaultAsync();

        if (product is null)
        {
            _logger.LogWarning(
                "Ürün bulunamadı. ProductId: {ProductId}",
                id
            );

            return null;
        }

        return product;
    }

    public async Task<ProductResponseDto> CreateAsync(
        ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        ClearProductCache();

        _logger.LogInformation(
            "Ürün oluşturuldu. ProductId: {ProductId}, Name: {ProductName}",
            product.Id,
            product.Name
        );

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductUpdateDto dto)
    {
        var product =
            await _context.Products.FindAsync(id);

        if (product is null)
        {
            _logger.LogWarning(
                "Güncellenecek ürün bulunamadı. ProductId: {ProductId}",
                id
            );

            return false;
        }

        product.Name = dto.Name;
        product.Price = dto.Price;

        await _context.SaveChangesAsync();

        ClearProductCache();

        _logger.LogInformation(
            "Ürün güncellendi. ProductId: {ProductId}",
            product.Id
        );

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product =
            await _context.Products.FindAsync(id);

        if (product is null)
        {
            _logger.LogWarning(
                "Silinecek ürün bulunamadı. ProductId: {ProductId}",
                id
            );

            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        ClearProductCache();

        _logger.LogInformation(
            "Ürün silindi. ProductId: {ProductId}",
            id
        );

        return true;
    }

    private void ClearProductCache()
    {
        _productCacheTokenSource.Cancel();
        _productCacheTokenSource.Dispose();

        _productCacheTokenSource =
            new CancellationTokenSource();

        _logger.LogInformation(
            "Product cache temizlendi."
        );
    }
}