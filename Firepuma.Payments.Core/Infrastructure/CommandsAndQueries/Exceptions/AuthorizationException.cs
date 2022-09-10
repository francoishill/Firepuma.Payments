namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;

public class AuthorizationException : Exception
{
    public AuthorizationException(string message)
        : base(message)
    {
    }
}