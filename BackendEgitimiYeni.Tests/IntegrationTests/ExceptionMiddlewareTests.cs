using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BackendEgitimiYeni.Tests.IntegrationTests;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Middleware_ShouldReturn500_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var testSettings =
                        new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] =
                                "BackendEgitimiYeni-Integration-Test-Key-2026-123456789",

                            ["Jwt:Issuer"] =
                                "BackendEgitimiYeni",

                            ["Jwt:Audience"] =
                                "BackendEgitimiYeniUsers"
                        };

                    config.AddInMemoryCollection(testSettings);
                });

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProductService>();

                    services.AddScoped<
                        IProductService,
                        ThrowingProductService>();

                    services
                        .AddAuthentication("Test")
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            TestAuthHandler>(
                            "Test",
                            options =>
                            {
                            });

                    services.AddAuthorization();
                });
            });

        var client = factory.CreateClient();

        // Act
        var response =
            await client.GetAsync("/api/products");

        // Assert
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode
        );

        var content =
            await response.Content.ReadAsStringAsync();

        using var json =
            JsonDocument.Parse(content);

        Assert.Equal(
            500,
            json.RootElement
                .GetProperty("statusCode")
                .GetInt32()
        );

        Assert.Equal(
            "Sunucuda beklenmeyen bir hata oluştu.",
            json.RootElement
                .GetProperty("message")
                .GetString()
        );
    }

    private class ThrowingProductService : IProductService
    {
        public Task<List<ProductResponseDto>> GetAllAsync()
        {
            throw new Exception("Test exception");
        }

        public Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductResponseDto> CreateAsync(
            ProductCreateDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(
            int id,
            ProductUpdateDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

    private class TestAuthHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    "1"
                ),

                new Claim(
                    ClaimTypes.Name,
                    "TestUser"
                )
            };

            var identity =
                new ClaimsIdentity(
                    claims,
                    "Test"
                );

            var principal =
                new ClaimsPrincipal(identity);

            var ticket =
                new AuthenticationTicket(
                    principal,
                    "Test"
                );

            return Task.FromResult(
                AuthenticateResult.Success(ticket)
            );
        }
    }
}