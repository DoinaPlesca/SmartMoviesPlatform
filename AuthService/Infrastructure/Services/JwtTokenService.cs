using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;

    public JwtTokenService(string secretKey, string issuer)
    {
        _secretKey = secretKey;
        _issuer = issuer;
    }

    public AuthenticationToken CreateToken(Dictionary<string, string> claims, TimeSpan? lifetime = null)
    {
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //var jwtClaims = claims.Select(c => new Claim(c.Key, c.Value)).ToList();
        
        var jwtClaims = new List<Claim>
        {
            new Claim("iss", _issuer) // REQUIRED for Kong to validate(adds the issuer claim, which must match the key I give Kong in the consumer config)
        };
        
        
        foreach (var pair in claims)
        {
            jwtClaims.Add(new Claim(pair.Key, pair.Value));
        }


        var token = new JwtSecurityToken(
            issuer: _issuer,
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