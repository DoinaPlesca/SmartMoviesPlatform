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
                    new Genre { Name = "Sci-Fi" },
                    new Genre { Name = "Comedy" },
                    new Genre { Name = "Horror" },
                    new Genre { Name = "Romance" },
                    new Genre { Name = "Thriller" },
                    new Genre { Name = "Animation" },
                    new Genre { Name = "Family" },
                    new Genre { Name = "Mystery" },
                    new Genre { Name = "Adventure" },
                    new Genre { Name = "Fantasy" },
                    new Genre { Name = "History" },
                    new Genre { Name = "Music" },
                    new Genre { Name = "War" },
                    new Genre { Name = "Western" },
                    new Genre { Name = "Documentary" }
                        
                );

                await context.SaveChangesAsync();
            }
        }
    }
}