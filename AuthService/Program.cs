using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Security;
using AuthService.Infrastructure.Services;
using DotNetEnv;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../.env");

var envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
 
var postgresConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY not set");


builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));


//dependencies
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<AuthService.Application.Services.AuthService>();
builder.Services.AddSingleton<JwtTokenService>(sp =>
{
    var key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
              ?? throw new InvalidOperationException("JWT_SECRET_KEY is missing!");
    return new JwtTokenService(key);
});
builder.Services.AddAutoMapper(typeof(Program));

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); 

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();