using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionAppManager.Api.ServiceBusTriggers;

public class ProcessDeadLetterMessage
{
    private readonly IDeadLetteredMessageRepository _deadLetteredMessageRepository;

    public ProcessDeadLetterMessage(
        IDeadLetteredMessageRepository deadLetteredMessageRepository)
    {
        _deadLetteredMessageRepository = deadLetteredMessageRepository;
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
            log.LogInformation(
                "Writing dead lettered message to CosmosDb, message enqueued on {Enqueued} with ID {MessageId}",
                message.EnqueuedTime.ToString("O"), message.MessageId);

            var deadLetteredMessageEntity = new DeadLetteredMessage
            {
                MessageId = message.MessageId,

                EnqueuedTime = message.EnqueuedTime,
                EnqueuedYearAndMonth = $"{message.EnqueuedTime.Year}{message.EnqueuedTime.Month:D2}",

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

            await _deadLetteredMessageRepository.AddItemAsync(deadLetteredMessageEntity, cancellationToken);

            log.LogInformation("Successfully wrote message ID {MessageId} to CosmosDb", message.MessageId);
        }
        catch (Exception exception)
        {
            log.LogError(exception, "Unable to process deadlettered message, exception: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);

            throw;
        }
    }
}