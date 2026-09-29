using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public class AiToolExecutor : IAiToolExecutor
{
    private readonly IProductService _productService;
    private readonly ILogger<AiToolExecutor> _logger;

    public AiToolExecutor(
        IProductService productService,
        ILogger<AiToolExecutor> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<object> ExecuteGetProductsAsync(
        GetProductsToolArgumentsDto arguments,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(arguments);

        _logger.LogInformation(
            "get_products çalıştırılıyor. Search: {Search}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}",
            arguments.Search,
            arguments.MinPrice,
            arguments.MaxPrice
        );

        var query = new ProductQueryDto
        {
            Page = 1,
            PageSize = 10,
            Search = arguments.Search,
            MinPrice = arguments.MinPrice,
            MaxPrice = arguments.MaxPrice
        };

        var result =
            await _productService.GetAllAsync(
                query
            );

        return result.Items;
    }

    private static void ValidateArguments(
        GetProductsToolArgumentsDto arguments)
    {
        if (
            arguments.MinPrice.HasValue &&
            arguments.MinPrice.Value < 0
        )
        {
            throw new ArgumentException(
                "Minimum fiyat negatif olamaz."
            );
        }

        if (
            arguments.MaxPrice.HasValue &&
            arguments.MaxPrice.Value < 0
        )
        {
            throw new ArgumentException(
                "Maksimum fiyat negatif olamaz."
            );
        }

        if (
            arguments.MinPrice.HasValue &&
            arguments.MaxPrice.HasValue &&
            arguments.MinPrice.Value >
            arguments.MaxPrice.Value
        )
        {
            throw new ArgumentException(
                "Minimum fiyat maksimum fiyattan büyük olamaz."
            );
        }
    }
}