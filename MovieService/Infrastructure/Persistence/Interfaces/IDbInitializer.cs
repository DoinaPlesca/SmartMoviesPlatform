namespace MovieService.Infrastructure.Persistence.Interfaces
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}