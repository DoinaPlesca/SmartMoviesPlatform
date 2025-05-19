using AuthService.Application.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../.env");


var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY not set");

builder.Services.AddSingleton(new JwtTokenService(secretKey));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build(); 

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();