﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.FunctionAppManager.Commands;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            try
            {
                await _mediator.Send(addCommand, cancellationToken);
            }
            catch (CommandException commandException)
            {
                log.LogCritical(
                    "Failed to process dead letter message, status {Status}, errors {Errors}",
                    commandException.StatusCode.ToString(), JsonConvert.SerializeObject(commandException.Errors));

                throw;
            }
        }
        catch (Exception exception)
        {
            const int pauseDurationInSeconds = 60;

            log.LogError(
                exception,
                "Unable to process deadlettered message, pausing for {PauseDuration} seconds to avoid infinite loop, exception: {Message}, stack trace: {Stack}",
                pauseDurationInSeconds, exception.Message, exception.StackTrace);

            //TODO: figure out how to achieve a circuit breaker
            //  We need a circuit breaker here instead because CosmosDb is a failure point and we try to
            //  also write dead lettered messages to Cosmos. So if it initially ends up dead lettered due to a CosmosDb
            //  failure and CosmosDb is down, it will fail to process the dead letter message and keep on retrying
            await Task.Delay(TimeSpan.FromSeconds(pauseDurationInSeconds), cancellationToken);

            throw;
        }
    }
}