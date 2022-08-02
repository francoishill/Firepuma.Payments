using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Firepuma.PaymentsService.FunctionAppManager.Infrastructure.PipelineBehaviors;

public class PerformanceLogBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceLogBehavior<TRequest, TResponse>> _logger;

    public PerformanceLogBehavior(ILogger<PerformanceLogBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestTypeName = typeof(TRequest).GetShortTypeName();

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Starting request {Name}",
            requestTypeName);

        var response = await next();

        stopwatch.Stop();
        var durationInSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F");

        var responseTypeName = typeof(TResponse).GetShortTypeName();
        _logger.LogInformation(
            "Finished request {Name} (with response type {ResponseType}) in {Duration}s",
            requestTypeName, responseTypeName, durationInSeconds);

        return response;
    }
}