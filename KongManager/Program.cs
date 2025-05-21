using System.Text.Json;
using KongManager.Models;
using KongManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

var kongBaseUrl = builder.Configuration["Kong:BaseUrl"] ?? "http://localhost:8001";

builder.Services.AddHttpClient("KongAdmin", client =>
{
    client.BaseAddress = new Uri(kongBaseUrl);
});
builder.Services.AddScoped<KongAdminService>();

var app = builder.Build();



// Load Kong route config from file
var configPath = Path.Combine("Configuration", "kong.routes.json");
var configJson = await File.ReadAllTextAsync(configPath);
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var kongConfig = JsonSerializer.Deserialize<KongConfigRoot>(configJson, options)!;

using var scope = app.Services.CreateScope();
var kong = scope.ServiceProvider.GetRequiredService<KongAdminService>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();


// Delete all existing routes before re-registering
logger.LogInformation("Deleting all existing Kong routes and services...");
await kong.DeleteAllRoutesAsync();
await kong.DeleteAllServicesAsync();
logger.LogInformation("All routes and services deleted.");
// await kong.EnableGlobalFileLogAsync("/dev/stdout");



// Register services, routes, and plugins
foreach (var svc in kongConfig.services)
{
    logger.LogInformation("Registering service: {Name}", svc.name);
    await kong.RegisterServiceAsync(svc.name, svc.url);

    foreach (var route in svc.routes)
    {
        var path = route.paths.First();
        logger.LogInformation("Registering route: {Name} → {Path}", route.name, path);
        await kong.RegisterRouteAsync(svc.name, path, route.methods);

        var routeId = await kong.GetRouteIdByPathAsync(path);
        if (!string.IsNullOrEmpty(routeId) && route.plugins != null)
        {
            foreach (var plugin in route.plugins)
            {
                logger.LogInformation("Adding plugin: {Plugin}", plugin.Name);
                await kong.AddPluginToRouteAsync(routeId, plugin);
            }
        }
    }
}

logger.LogInformation("Kong configuration completed. JWT validation enabled via plugins.");
app.Run();
