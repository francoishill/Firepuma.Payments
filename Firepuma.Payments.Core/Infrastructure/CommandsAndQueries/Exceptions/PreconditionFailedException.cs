namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;

public class PreconditionFailedException : Exception
{
    public PreconditionFailedException(string message)
        : base(message)
    {
    }
}