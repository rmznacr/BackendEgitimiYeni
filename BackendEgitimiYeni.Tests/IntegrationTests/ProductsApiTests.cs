using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BackendEgitimiYeni.Tests.IntegrationTests;

public class ProductsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync(
        string role = "User")
    {
        var username =
            role.ToLower() + "_" + Guid.NewGuid();

        var password = "Test123456";

        var registerDto = new RegisterDto
        {
            Username = username,
            Password = password
        };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerDto
            );

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode
        );

        if (role == "Admin")
        {
            using var scope =
                _factory.Services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user = await context.Users
                .FirstAsync(u => u.Username == username);

            user.Role = "Admin";

            await context.SaveChangesAsync();
        }

        var loginDto = new LoginDto
        {
            Username = username,
            Password = password
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginDto
            );

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode
        );

        var result =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);

        Assert.False(
            string.IsNullOrWhiteSpace(result.Token)
        );

        return result.Token;
    }

    private void SetToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );
    }

    [Fact]
    public async Task GetProducts_WithoutToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response =
            await _client.GetAsync("/api/products");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetProducts_WithUserToken_ShouldReturnOk()
    {
        var token =
            await RegisterAndLoginAsync("User");

        SetToken(token);

        var response =
            await _client.GetAsync("/api/products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    [Fact]
    public async Task CreateProduct_WithUserRole_ShouldReturnForbidden()
    {
        var token =
            await RegisterAndLoginAsync("User");

        SetToken(token);

        var product = new ProductCreateDto
        {
            Name = "User Product",
            Price = 1000
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/products",
                product
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task CreateProduct_WithAdminRole_ShouldReturnCreated()
    {
        var token =
            await RegisterAndLoginAsync("Admin");

        SetToken(token);

        var product = new ProductCreateDto
        {
            Name = "Admin Product",
            Price = 25000
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/products",
                product
            );

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );

        var createdProduct =
            await response.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        Assert.Equal(
            "Admin Product",
            createdProduct.Name
        );

        Assert.Equal(
            25000,
            createdProduct.Price
        );
    }

    [Fact]
    public async Task UpdateProduct_WithAdminRole_ShouldReturnNoContent()
    {
        var token =
            await RegisterAndLoginAsync("Admin");

        SetToken(token);

        var createDto = new ProductCreateDto
        {
            Name = "Old Product",
            Price = 1000
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/products",
                createDto
            );

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode
        );

        var createdProduct =
            await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        var updateDto = new ProductUpdateDto
        {
            Name = "Updated Product",
            Price = 2000
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/api/products/{createdProduct.Id}",
                updateDto
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );

        var getResponse =
            await _client.GetAsync(
                $"/api/products/{createdProduct.Id}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
        );

        var updatedProduct =
            await getResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(updatedProduct);

        Assert.Equal(
            "Updated Product",
            updatedProduct.Name
        );

        Assert.Equal(
            2000,
            updatedProduct.Price
        );
    }

    [Fact]
    public async Task DeleteProduct_WithAdminRole_ShouldReturnNoContent()
    {
        var token =
            await RegisterAndLoginAsync("Admin");

        SetToken(token);

        var createDto = new ProductCreateDto
        {
            Name = "Product To Delete",
            Price = 500
        };

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/products",
                createDto
            );

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode
        );

        var createdProduct =
            await createResponse.Content
                .ReadFromJsonAsync<ProductResponseDto>();

        Assert.NotNull(createdProduct);

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/products/{createdProduct.Id}"
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

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