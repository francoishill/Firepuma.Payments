﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
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
    public class Command : BaseCommand, IRequest<Result>
    {
        public string MessageId { get; init; }
        public DateTimeOffset EnqueuedTime { get; init; }
        public string MessageBody { get; init; }
        public string Subject { get; init; }
        public string ContentType { get; init; }
        public string CorrelationId { get; init; }
        public int DeliveryCount { get; init; }
        public string PartitionKey { get; init; }
        public string SessionId { get; init; }
        public string DeadLetterReason { get; init; }
        public string DeadLetterSource { get; init; }
        public string DeadLetterErrorDescription { get; init; }
        public Dictionary<string, object> ApplicationProperties { get; init; }
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