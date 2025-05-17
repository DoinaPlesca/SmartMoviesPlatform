using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternalGateway.Application.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
namespace InternalGateway.Application.Services;

public class JwtTokenService
{
    private readonly string _secret;

    public JwtTokenService(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("JWT secret cannot be null or empty.", nameof(secret));

        _secret = secret;
    }

    public AuthenticationToken CreateToken(CreateTokenOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = options.Claims.Select(pair => new Claim(pair.Key, pair.Value)).ToList();

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(options.Lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds
        );

        return new AuthenticationToken
        {
            Value = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}