using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MovieService.Application.Common.Interfaces;
using MovieService.Application.Services;
using MovieService.Infrastructure.Messaging;
using MovieService.Infrastructure.Persistence;
using MovieService.Infrastructure.Persistence.Initialization;
using MovieService.Infrastructure.Persistence.Interfaces;
using MovieService.Infrastructure.Persistence.Repositories;
using MovieService.Infrastructure.Persistence.Seeders;
using MovieService.Infrastructure.Storage;
using SharedKernel.Interfaces;
using SharedKernel.Middleware;


var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddOpenApi();

var envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
 DotNetEnv.Env.Load(envPath);
 
var postgresConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

 if (string.IsNullOrWhiteSpace(postgresConnectionString))
     throw new Exception("Environment variable DB_CONNECTION_STRING is missing!");

Console.WriteLine($"Connecting to PostgreSQL with: {postgresConnectionString}");

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));


builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IDataSeeder, GenreSeeder>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService.Application.Services.MovieService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();


builder.Services.AddAutoMapper(typeof(Program));

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

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


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();