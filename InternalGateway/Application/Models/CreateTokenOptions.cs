namespace InternalGateway.Application.Models;

public class CreateTokenOptions
{
    public Dictionary<string, string> Claims { get; set; } = new();
    public TimeSpan? Lifetime { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}