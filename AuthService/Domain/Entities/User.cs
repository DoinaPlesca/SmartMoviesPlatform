using AuthService.Application.Interfaces;

namespace AuthService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string Email { get; private set; }

    private User() { }

    public User(string username, string passwordHash, string email)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
    }

    public bool VerifyPassword(string password, IPasswordHasher hasher)
    {
        return hasher.Verify(password, PasswordHash);
    }
}