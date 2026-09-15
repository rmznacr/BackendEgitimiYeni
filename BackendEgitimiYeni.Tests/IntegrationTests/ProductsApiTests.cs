using BackendEgitimiYeni.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace BackendEgitimiYeni.Tests.IntegrationTests;

public class ProductsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated()
    {
        // Arrange
        var product = new ProductCreateDto
        {
            Name = "Test Laptop",
            Price = 25000
        };

        // Act
        var response =
            await _client.PostAsJsonAsync("/api/products", product);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct =
            await response.Content.ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);
        Assert.Equal("Test Laptop", createdProduct.Name);
        Assert.Equal(25000, createdProduct.Price);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct()
    {
        // Arrange
        var product = new ProductCreateDto
        {
            Name = "Test Phone",
            Price = 15000
        };

        var createResponse =
            await _client.PostAsJsonAsync("/api/products", product);

        var createdProduct =
            await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        // Act
        var response =
            await _client.GetAsync(
                $"/api/products/{createdProduct.Id}"
            );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedProduct =
            await response.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(returnedProduct);
        Assert.Equal(createdProduct.Id, returnedProduct.Id);
        Assert.Equal("Test Phone", returnedProduct.Name);
        Assert.Equal(15000, returnedProduct.Price);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNoContent()
    {
        // Arrange
        var product = new ProductCreateDto
        {
            Name = "Old Product",
            Price = 1000
        };

        var createResponse =
            await _client.PostAsJsonAsync("/api/products", product);

        var createdProduct =
            await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        var updateDto = new ProductUpdateDto
        {
            Name = "Updated Product",
            Price = 2000
        };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/products/{createdProduct.Id}",
                updateDto
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );

        // Güncellemenin gerçekten yapıldığını kontrol et
        var getResponse =
            await _client.GetAsync(
                $"/api/products/{createdProduct.Id}"
            );

        var updatedProduct =
            await getResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product", updatedProduct.Name);
        Assert.Equal(2000, updatedProduct.Price);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent()
    {
        // Arrange
        var product = new ProductCreateDto
        {
            Name = "Product To Delete",
            Price = 500
        };

        var createResponse =
            await _client.PostAsJsonAsync("/api/products", product);

        var createdProduct =
            await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        // Act
        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/products/{createdProduct.Id}"
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        // Gerçekten silindiğini kontrol et
        var getResponse =
            await _client.GetAsync(
                $"/api/products/{createdProduct.Id}"
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }
}