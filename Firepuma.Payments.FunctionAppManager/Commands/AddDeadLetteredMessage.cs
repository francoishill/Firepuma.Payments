using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class AddDeadLetteredMessage
{
    public class Command : BaseCommand<Result>
    {
        public string MessageId { get; init; } = null!;
        public DateTimeOffset EnqueuedTime { get; init; }
        public string MessageBody { get; init; } = null!;
        public string Subject { get; init; } = null!;
        public string ContentType { get; init; } = null!;
        public string CorrelationId { get; init; } = null!;
        public int DeliveryCount { get; init; }
        public string PartitionKey { get; init; } = null!;
        public string SessionId { get; init; } = null!;
        public string DeadLetterReason { get; init; } = null!;
        public string DeadLetterSource { get; init; } = null!;
        public string DeadLetterErrorDescription { get; init; } = null!;
        public Dictionary<string, object> ApplicationProperties { get; init; } = null!;
    }

    public class Result
    {
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IDeadLetteredMessageRepository _deadLetteredMessageRepository;

        public Handler(
            ILogger<Handler> logger,
            IDeadLetteredMessageRepository deadLetteredMessageRepository)
        {
            _logger = logger;
            _deadLetteredMessageRepository = deadLetteredMessageRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var message = new DeadLetteredMessage
            {
                MessageId = command.MessageId,

                EnqueuedTime = command.EnqueuedTime,
                EnqueuedYearAndMonth = $"{command.EnqueuedTime.Year}{command.EnqueuedTime.Month:D2}",

                MessageBody = command.MessageBody,

                Subject = command.Subject,
                ContentType = command.ContentType,
                CorrelationId = command.CorrelationId,
                DeliveryCount = command.DeliveryCount,
                PartitionKey = command.PartitionKey,
                SessionId = command.SessionId,
                DeadLetterReason = command.DeadLetterReason,
                DeadLetterSource = command.DeadLetterSource,
                DeadLetterErrorDescription = command.DeadLetterErrorDescription,
                ApplicationProperties = command.ApplicationProperties,
            };

            _logger.LogInformation(
                "Writing dead lettered message to CosmosDb, message enqueued on {Enqueued} with ID {MessageId}",
                message.EnqueuedTime.ToString("O"), message.MessageId);

            await _deadLetteredMessageRepository.AddItemAsync(message, cancellationToken);

            _logger.LogInformation("Successfully wrote message ID {MessageId} to CosmosDb", message.MessageId);

            return new Result();
        }
    }
}