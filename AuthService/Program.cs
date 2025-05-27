using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Security;
using AuthService.Infrastructure.Services;
using DotNetEnv;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using SharedKernel.Middleware;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://seq") 
    .WriteTo.Console()
    .CreateLogger();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

Log.Information("🚀 Starting up AuthService...");

Env.Load("../.env");

var postgresConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                               ?? throw new InvalidOperationException("DB_CONNECTION_STRING is not set");

var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY is not set");

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
             ?? throw new InvalidOperationException("JWT_ISSUER is not set");


builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));


//dependencies
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<AuthService.Application.Services.AuthService>();
builder.Services.AddSingleton(new JwtTokenService(secretKey, issuer));
builder.Services.AddAutoMapper(typeof(Program));

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuthService"))
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
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuthService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();;
    });


var app = builder.Build(); 

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseMetricServer();
app.UseHttpMetrics();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();