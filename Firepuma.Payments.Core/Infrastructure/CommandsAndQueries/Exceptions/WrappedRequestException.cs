using System.Net;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;

public class WrappedRequestException : Exception
{
    public HttpStatusCode StatusCode { get; set; }
    public Error[] Errors { get; set; }

    public WrappedRequestException(
        HttpStatusCode statusCode,
        Exception innerException,
        params Error[] errors)
        : base($"Status: {statusCode.ToString()}, Errors: {string.Join(", ", errors.Select(e => $"{e.Code} {e.Message}"))}", innerException)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    } 
}