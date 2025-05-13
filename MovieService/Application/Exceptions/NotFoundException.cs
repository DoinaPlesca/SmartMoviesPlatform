namespace MovieService.Application.Exceptions_;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}