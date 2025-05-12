using Microsoft.EntityFrameworkCore;
using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Persistence
{
    public class DbInitializer : IDbInitializer
    {
        public async Task InitializeAsync(MovieDbContext context)
        {
            await context.Database.MigrateAsync();

            // Only seed if the table is empty
            if (!context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Name = "Action" },
                    new Genre { Name = "Drama" },
                    new Genre { Name = "Sci-Fi" }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}