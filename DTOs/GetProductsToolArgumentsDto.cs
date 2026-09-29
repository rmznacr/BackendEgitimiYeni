namespace BackendEgitimiYeni.DTOs;

public class GetProductsToolArgumentsDto
{
    public string? Search { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }
}