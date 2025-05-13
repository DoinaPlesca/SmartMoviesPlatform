using MovieService.Application.Common.Interfaces;
using MovieService.Infrastructure.Persistence.Interfaces;

namespace MovieService.Infrastructure.Persistence.Initialization;

public class DbInitializer : IDbInitializer
{
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly MovieDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IEnumerable<IDataSeeder> seeders, MovieDbContext context, ILogger<DbInitializer> logger)
    {
        _seeders = seeders;
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Running database migrations...");
        await _context.Database.EnsureCreatedAsync(); 
        _logger.LogInformation("Database ready.");

        foreach (var seeder in _seeders)
        {
            try
            {
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding data.");
                throw;
            }
        }

        _logger.LogInformation("All seeders completed.");
    }
}