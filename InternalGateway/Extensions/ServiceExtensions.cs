using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using SharedKernel.Extensions;

namespace InternalGateway.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        DotNetEnv.Env.Load();
        
        services.AddJwtAuthentication(configuration);

        services.AddOcelot(configuration).AddPolly();

        return services;
    }
}