using System;
using System.Runtime.Serialization;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.Exceptions;

[Serializable]
public class ApplicationSecretInvalidException : Exception
{
    public ApplicationSecretInvalidException()
    {
    }

    public ApplicationSecretInvalidException(string message)
        : base(message)
    {
    }

    public ApplicationSecretInvalidException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected ApplicationSecretInvalidException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context)
    {
    }
}