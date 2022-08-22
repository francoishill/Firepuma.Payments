using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.PipelineBehaviors
{
    public class AuditCommandsBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<AuditCommandsBehaviour<TRequest, TResponse>> _logger;
        private readonly ITableProvider<CommandExecutionEvent> _commandExecutionTableProvider;

        public AuditCommandsBehaviour(
            ILogger<AuditCommandsBehaviour<TRequest, TResponse>> logger,
            ITableProvider<CommandExecutionEvent> commandExecutionTableProvider)
        {
            _logger = logger;
            _commandExecutionTableProvider = commandExecutionTableProvider;
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
            await _commandExecutionTableProvider.AddEntityAsync(executionEvent, cancellationToken);

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

            await _commandExecutionTableProvider.UpdateEntityAsync(executionEvent, ETag.All, TableUpdateMode.Replace, cancellationToken);

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