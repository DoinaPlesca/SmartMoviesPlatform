using KongManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// HTTP client for Kong Admin API
builder.Services.AddHttpClient("KongAdmin", client =>
{
    client.BaseAddress = new Uri("http://kong:8001");
});

// Register services
builder.Services.AddScoped<KongAdminService>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var kong = scope.ServiceProvider.GetRequiredService<KongAdminService>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();


// Load secrets from .env
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtSecret = builder.Configuration["JWT_SECRET_KEY"];

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Missing JWT_ISSUER or JWT_SECRET_KEY in environment.");


// 1: Register service
await kong.RegisterServiceAsync("smart-ocelot", "http://api-gateway:80");
logger.LogInformation(" Registered service: smart-ocelot");


// 2: Register routes and attach plugins
var paths = new[]
{
    "/api",
    "/api/auth",
    "/api/movies",
    "/api/watchlist",
    "/api/genres"
};

foreach (var path in paths)
{
    await kong.RegisterRouteAsync("smart-ocelot", path);
    var routeId = await kong.GetRouteIdByPathAsync(path);

    if (!string.IsNullOrEmpty(routeId))
    {
        logger.LogInformation("Configuring route: {Path}", path);
        await kong.AddHttpLogPluginToRouteAsync(routeId);
        await kong.AddRateLimitPluginToRouteAsync(routeId, 60);
        await kong.AddJwtPluginToRouteAsync(routeId);
        await kong.AddCorsPluginToRouteAsync(routeId);
        logger.LogInformation(" Plugins attached to: {Path}", path);
    }
    else
    {
        logger.LogWarning(" Route ID not found for path: {Path}", path);
    }
}

// 3: Create consumer
logger.LogInformation(" Creating Kong consumer and JWT credentials...");

await kong.CreateConsumerAsync("smart-client");
await kong.CreateJwtCredentialAsync("smart-client", jwtIssuer!, jwtSecret!);

logger.LogInformation(" JWT consumer and credentials configured");

app.UseHttpsRedirection();
app.Run();
