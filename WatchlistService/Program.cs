
using SharedKernel.Middleware;
using WatchlistService.Application.Interfaces;
using WatchlistService.Application.Services;
using WatchlistService.Infrastructure;
using WatchlistService.Infrastructure.Mappings;
using WatchlistService.Infrastructure.Messaging;
using WatchlistService.Infrastructure.Persistance.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IMovieCacheRepository, MovieCacheRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IWatchlistService, WatchlistService.Application.Services.WatchlistService>();
builder.Services.AddAutoMapper(typeof(WatchlistMappingProfile).Assembly);

builder.Services.AddHostedService<MovieCreatedConsumer>();
builder.Services.AddHostedService<MovieUpdatedConsumer>();
builder.Services.AddHostedService<MovieDeletedConsumer>();

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
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();