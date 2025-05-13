namespace MovieService.Infrastructure.Persistence
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}