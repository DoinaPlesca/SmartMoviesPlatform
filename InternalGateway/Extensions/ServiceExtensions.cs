using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

namespace InternalGateway.Extensions;

public static class ServiceExtensions
{

    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["JWT_SECRET_KEY"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException("JWT_SECRET_KEY is missing. Please set it in .env or environment variables.");
        }

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        services.AddOcelot(configuration).AddPolly();
        return services;
    }

    public static WebApplication UseApiGateway(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOcelot().Wait();
        return app;
    }
}
