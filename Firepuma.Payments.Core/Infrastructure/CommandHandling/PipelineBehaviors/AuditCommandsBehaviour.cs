using Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.Payments.Core.Infrastructure.CommandHandling.PipelineBehaviors
{
    public class AuditCommandsBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<AuditCommandsBehaviour<TRequest, TResponse>> _logger;
        private readonly ICommandExecutionEventRepository _commandExecutionEventRepository;

        public AuditCommandsBehaviour(
            ILogger<AuditCommandsBehaviour<TRequest, TResponse>> logger,
            ICommandExecutionEventRepository commandExecutionEventRepository)
        {
            _logger = logger;
            _commandExecutionEventRepository = commandExecutionEventRepository;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            if (request is BaseCommand baseCommand)
            {
                return await ExecuteAndAudit(request, next, baseCommand, cancellationToken);
            }

            return await next();
        }

        private async Task<TResponse> ExecuteAndAudit(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            BaseCommand baseCommand,
            CancellationToken cancellationToken)
        {
            var executionEvent = new CommandExecutionEvent(baseCommand);
            await _commandExecutionEventRepository.AddItemAsync(executionEvent, cancellationToken);

            var startTime = DateTime.UtcNow;

            TResponse response = default;

            string result = null;
            Exception error = null;
            bool successful;
            try
            {
                response = await next();
                result = response != null ? JsonConvert.SerializeObject(response, GetAuditResponseSerializerSettings()) : null;
                successful = true;
            }
            catch (Exception exception)
            {
                error = exception;
                successful = false;
                _logger.LogError(exception, "Failed to execute command type {Type}", request.GetType().FullName);
            }

            var finishedTime = DateTime.UtcNow;

            executionEvent.Successful = successful;
            executionEvent.Result = result;
            executionEvent.ErrorMessage = error?.Message;
            executionEvent.ErrorStackTrack = error?.StackTrace;
            executionEvent.ExecutionTimeInSeconds = (finishedTime - startTime).TotalSeconds;
            executionEvent.TotalTimeInSeconds = (finishedTime - baseCommand.CreatedOn).TotalSeconds;

            await _commandExecutionEventRepository.UpdateItemAsync(executionEvent, cancellationToken);

            if (error != null)
            {
                throw error;
            }

            return response;
        }

        private static JsonSerializerSettings GetAuditResponseSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return jsonSerializerSettings;
        }
    }
}