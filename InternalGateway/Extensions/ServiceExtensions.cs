using System.IdentityModel.Tokens.Jwt;
using System.Text;
using InternalGateway.Application.Services;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace InternalGateway.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddJwtTokenService(this IServiceCollection services, IConfiguration config)
    {
        var secretKey = GetJwtSecret(config);
        services.AddSingleton(new JwtTokenService(secretKey));
        return services;
    }

    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, IConfiguration config)
    {
        var secretKey = GetJwtSecret(config);

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        services.AddOcelot(config);
        return services;
    }

    public static WebApplication UseApiGateway(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOcelot().Wait();
        return app;
    }

    private static string GetJwtSecret(IConfiguration config)
    {
        var secret = config["AuthenticationProviderKey"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT secret key is missing in configuration. Please set 'AuthenticationProviderKey'.");
        return secret;
    }
}