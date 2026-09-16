using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendEgitimiYeni.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var registered = await _authService.RegisterAsync(dto);

        if (!registered)
        {
            return Conflict(new
            {
                message = "Bu kullanıcı adı zaten kullanılıyor."
            });
        }

        return StatusCode(201, new
        {
            message = "Kullanıcı başarıyla oluşturuldu."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Kullanıcı adı veya parola hatalı."
            });
        }

        return Ok(result);
    }
}