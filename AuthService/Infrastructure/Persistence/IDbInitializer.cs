namespace AuthService.Infrastructure.Persistence;

public interface IDbInitializer
{
    Task InitializeAsync();
}