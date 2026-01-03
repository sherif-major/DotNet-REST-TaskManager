using Api.DTOs.Auth;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);

        if (!result.ok)
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid username or password"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            token = result.token,
            username = result.username,
            role = result.role
        }, "Login successful"));
    }
}
