using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly AuthDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(AuthDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying pending migrations...");

        await _context.Database.MigrateAsync();

        _logger.LogInformation("Database is up to date.");
    }
}