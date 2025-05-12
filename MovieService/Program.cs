
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MovieService.Application.Services;
using MovieService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddOpenApi();
DotNetEnv.Env.Load();

var postgresConnectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
                               $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};" +
                               $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
                               $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                               $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};";

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService.Application.Services.MovieService>();


builder.Services.AddAutoMapper(typeof(Program));

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync(dbContext);
}


app.UseHttpsRedirection();

await app.RunAsync();