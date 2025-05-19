using AuthService.Application.Models;
using AuthService.Application.Services;

using Microsoft.AspNetCore.Mvc;

namespace InternalGateway.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtService;

    public AuthController(JwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        // Simulated user store
        var users = new List<(string Username, string Password, string Role)>
        {
            ("admin", "password", "Admin"),
            ("user", "user123", "User")
        };

        var matchedUser = users.FirstOrDefault(u =>
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)
            && u.Password == request.Password
        );

        if (matchedUser == default)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var claims = new Dictionary<string, string>
        {
            { "sub", Guid.NewGuid().ToString() },
            { "username", matchedUser.Username },
            { "role", matchedUser.Role }
        };

        var token = _jwtService.CreateToken(claims);

        var response = new LoginResponseDto
        {
            Token = token.Value,
            Username = matchedUser.Username
        };

        return Ok(response);
    }
}