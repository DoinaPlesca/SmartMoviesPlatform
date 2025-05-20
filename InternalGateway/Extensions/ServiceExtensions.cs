using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using SharedKernel.Extensions;

namespace InternalGateway.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        DotNetEnv.Env.Load();
        
        services.AddJwtAuthentication(configuration);
        
        // var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        //                 ?? throw new InvalidOperationException("JWT_SECRET_KEY is not set");
        //
        // var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
        //              ?? throw new InvalidOperationException("JWT_ISSUER is not set");
        //
        // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); 
        //
        // services.AddAuthentication("Bearer")
        //     .AddJwtBearer("Bearer", options =>
        //     {
        //         options.RequireHttpsMetadata = false;
        //         options.MapInboundClaims = false; 
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidIssuer = issuer,
        //             ValidateAudience = false,
        //             ValidateLifetime = true,
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        //            
        //         };
        //     });

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