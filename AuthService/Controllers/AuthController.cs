using AuthService.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;
using SharedKernel.Wrappers;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Application.Services.AuthService _authService;

    public AuthController(Application.Services.AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result)
            throw new BadRequestException("Username already exists.");

        return Ok(ApiResponse<string>.Ok("Registration successful"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
            throw new BadRequestException("Invalid username or password.");

        return Ok(ApiResponse<LoginResponseDto>.Ok(response));
    }
}