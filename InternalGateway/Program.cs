using InternalGateway.Extensions;
using Ocelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot + environment configs
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;

    config.SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddOcelot("Configurations", env as Microsoft.AspNetCore.Hosting.IWebHostEnvironment)
        .AddEnvironmentVariables();
});


// Register JWT validation + Ocelot
builder.Services.AddApiGatewayServices(builder.Configuration);

var app = builder.Build();

// Apply middleware pipeline
app.UseApiGateway();