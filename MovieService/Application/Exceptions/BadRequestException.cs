namespace MovieService.Application.Exceptions_;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}