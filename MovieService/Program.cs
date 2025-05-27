
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MovieService.Application.Common.Interfaces;
using MovieService.Application.Services;
using MovieService.FeatureToogles;
using MovieService.Infrastructure.Messaging;
using MovieService.Infrastructure.Persistence;
using MovieService.Infrastructure.Persistence.Initialization;
using MovieService.Infrastructure.Persistence.Interfaces;
using MovieService.Infrastructure.Persistence.Repositories;
using MovieService.Infrastructure.Persistence.Seeders;
using MovieService.Infrastructure.Storage;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using SharedKernel.Extensions;
using SharedKernel.Interfaces;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Logging (Serilog)
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "MovieService")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName) 
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341");
});



// Load .env file for environment variables
var envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables(); 

// PostgreSQL Connection
var postgresConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(postgresConnectionString))
     throw new Exception("Environment variable DB_CONNECTION_STRING is missing!");
Console.WriteLine($"Connecting to PostgreSQL with: {postgresConnectionString}");
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

// Dependency Injection
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IDataSeeder, GenreSeeder>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService.Application.Services.MovieService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// Feature Flags (loaded from .env)
builder.Services.Configure<FeatureFlags>(
    builder.Configuration.GetSection("FeatureFlags"));

// Other Services
builder.Services.AddAutoMapper(typeof(Program));
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();


// Swagger & Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});


// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MovieService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    })
    .WithMetrics(metricBuilder =>
    {
        metricBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MovieService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();;
    });


// App Pipeline
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMetricServer();
app.UseHttpMetrics();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Init Database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

// HTTP Request Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();