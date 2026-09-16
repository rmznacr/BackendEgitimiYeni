using BackendEgitimiYeni.DTOs;

namespace BackendEgitimiYeni.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto dto);

    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}