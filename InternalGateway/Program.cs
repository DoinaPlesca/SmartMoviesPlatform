using Ocelot.Middleware;
using InternalGateway.Extensions;
using Ocelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Enable verbose logging from Ocelot
builder.Logging.AddFilter("Ocelot.Middleware", LogLevel.Debug);
builder.Logging.AddFilter("Ocelot.Configuration", LogLevel.Debug);
builder.Logging.AddFilter("Ocelot.Errors", LogLevel.Debug);
builder.Logging.AddFilter("Ocelot.Responder", LogLevel.Debug);
builder.Logging.AddFilter("Ocelot.DownstreamRouteFinder", LogLevel.Debug);

// Optional: Reduce noise from Microsoft logs
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);


builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;
    config.SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json",  true,  true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true,  true)
        .AddJsonFile("Configuration/ocelot-auth.json",  true, true)
        .AddJsonFile("Configuration/ocelot-movie.json",  true, true)
        .AddJsonFile("Configuration/ocelot-watchlist.json",  true, true)
        .AddJsonFile("Configuration/ocelot-genre.json", true, true)
        .AddJsonFile("Configuration/ocelot-global.json", true, true)
        .AddOcelot("Configuration",env as IWebHostEnvironment)
        .AddEnvironmentVariables();
});




builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiGatewayServices(builder.Configuration); 
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => endpoints.MapControllers());

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" API Gateway started. Listening on /auth, /movies, /watchlist etc.");

var rawRoutes = builder.Configuration.GetSection("Routes").GetChildren();

foreach (var route in rawRoutes)
{
    var upstream = route["UpstreamPathTemplate"];
    var downstream = route["DownstreamPathTemplate"];
    logger.LogInformation("➡️ Configured Route: {upstream} → {downstream}", upstream, downstream);
}

try
{
    await app.UseOcelot();
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Ocelot failed to start or route properly");
}

app.Run();


