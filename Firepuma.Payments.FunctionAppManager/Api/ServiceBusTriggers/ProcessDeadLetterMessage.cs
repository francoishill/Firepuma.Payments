using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.FunctionAppManager.Commands;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionAppManager.Api.ServiceBusTriggers;

public class ProcessDeadLetterMessage
{
    private readonly IMediator _mediator;

    public ProcessDeadLetterMessage(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("ProcessDeadLetterMessage")]
    public async Task RunAsync(
        [ServiceBusTrigger("%QueueName%/$deadletterqueue", Connection = "ServiceBus")] ServiceBusReceivedMessage message,
        ILogger log,
        CancellationToken cancellationToken)
    {
        var correlationId = message.CorrelationId;
        log.LogInformation(
            "C# ServiceBus queue trigger function processing message with Id {Id} and correlationId {CorrelationId} at {Time}",
            message.MessageId, correlationId, DateTime.Now.ToString("O"));

        try
        {
            var addCommand = new AddDeadLetteredMessage.Command
            {
                MessageId = message.MessageId,
                EnqueuedTime = message.EnqueuedTime,
                MessageBody = message.Body.ToString(),
                Subject = message.Subject,
                ContentType = message.ContentType,
                CorrelationId = message.CorrelationId,
                DeliveryCount = message.DeliveryCount,
                PartitionKey = message.PartitionKey,
                SessionId = message.SessionId,
                DeadLetterReason = message.DeadLetterReason,
                DeadLetterSource = message.DeadLetterSource,
                DeadLetterErrorDescription = message.DeadLetterErrorDescription,
                ApplicationProperties = message.ApplicationProperties.ToDictionary(kv => kv.Key, kv => kv.Value),
            };

            var result = await _mediator.Send(addCommand, cancellationToken);

            if (!result.IsSuccessful)
            {
                log.LogError("{Reason}, {Errors}", result.FailedReason.ToString(), string.Join(", ", result.FailedErrors));
                throw new Exception($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
            }
        }
        catch (Exception exception)
        {
            log.LogError(exception, "Unable to process deadlettered message, exception: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);

            throw;
        }
    }
}