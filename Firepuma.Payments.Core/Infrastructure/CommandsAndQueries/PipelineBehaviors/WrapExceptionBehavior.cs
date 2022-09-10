using System.Net;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Firepuma.Payments.Core.Infrastructure.PipelineBehaviors.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.PipelineBehaviors;

public class WrapExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<WrapExceptionBehavior<TRequest, TResponse>> _logger;

    public WrapExceptionBehavior(
        ILogger<WrapExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestTypeName = BehaviorPipelineHelpers.GetShortTypeName(typeof(TRequest));

        try
        {
            return await next();
        }
        catch (ValidationException validationException)
        {
            _logger.LogError(
                validationException,
                "Failed to perform request {RequestType}, validation error was: {Error}",
                requestTypeName, validationException.Message);

            throw new WrappedRequestException(
                HttpStatusCode.BadRequest,
                validationException,
                validationException
                    .Errors.Select(e => new WrappedRequestException.Error
                    {
                        Code = HttpStatusCode.BadRequest.ToString(),
                        Message = e.ErrorMessage,
                    }).ToArray());
        }
        catch (AuthorizationException authorizationException)
        {
            _logger.LogError(
                authorizationException,
                "Failed to perform request {RequestType}, authorization error was: {Error}",
                requestTypeName, authorizationException.Message);

            throw new WrappedRequestException(
                HttpStatusCode.Forbidden,
                authorizationException,
                new WrappedRequestException.Error
                {
                    Code = HttpStatusCode.Forbidden.ToString(),
                    Message = authorizationException.Message,
                });
        }
        catch (PreconditionFailedException preconditionFailedException)
        {
            _logger.LogError(
                preconditionFailedException,
                "Failed to perform request {RequestType}, precondition failed error was: {Error}",
                requestTypeName, preconditionFailedException.Message);

            throw new WrappedRequestException(
                HttpStatusCode.PreconditionFailed,
                preconditionFailedException,
                new WrappedRequestException.Error
                {
                    Code = HttpStatusCode.PreconditionFailed.ToString(),
                    Message = preconditionFailedException.Message,
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to perform request {RequestType}, error was: {Error}",
                requestTypeName, exception.Message);

            throw;
        }
    }
}