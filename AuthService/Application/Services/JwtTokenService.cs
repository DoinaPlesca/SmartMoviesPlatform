using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services;

public class JwtTokenService
{
    private readonly string _secretKey;

    public JwtTokenService(string secretKey)
    {
        _secretKey = secretKey;
    }

    public AuthenticationToken CreateToken(Dictionary<string, string> claims, TimeSpan? lifetime = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtClaims = claims.Select(c => new Claim(c.Key, c.Value)).ToList();

        var token = new JwtSecurityToken(
            claims: jwtClaims,
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds
        );

        return new AuthenticationToken
        {
            Value = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}