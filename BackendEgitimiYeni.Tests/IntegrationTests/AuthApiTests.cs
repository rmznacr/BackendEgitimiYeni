using BackendEgitimiYeni.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace BackendEgitimiYeni.Tests.IntegrationTests;

public class AuthApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnCreated()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "register_" + Guid.NewGuid(),
            Password = "Test123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            dto
        );

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
    {
        // Arrange
        var username = "duplicate_" + Guid.NewGuid();

        var dto = new RegisterDto
        {
            Username = username,
            Password = "Test123456"
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            dto
        );

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode
        );

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            dto
        );

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode
        );
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        // Arrange
        var username = "login_" + Guid.NewGuid();

        var registerDto = new RegisterDto
        {
            Username = username,
            Password = "Test123456"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto
        );

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode
        );

        var loginDto = new LoginDto
        {
            Username = username,
            Password = "Test123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto
        );

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);
        Assert.False(
            string.IsNullOrWhiteSpace(result.Token)
        );
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var username = "wrongpassword_" + Guid.NewGuid();

        var registerDto = new RegisterDto
        {
            Username = username,
            Password = "Test123456"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto
        );

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode
        );

        var loginDto = new LoginDto
        {
            Username = username,
            Password = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto
        );

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }
}