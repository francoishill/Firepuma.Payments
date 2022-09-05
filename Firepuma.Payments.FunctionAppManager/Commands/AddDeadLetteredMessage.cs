using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.CommandHandling;
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
        public string MessageId { get; set; }
        public DateTimeOffset EnqueuedTime { get; set; }
        public string MessageBody { get; set; }
        public string Subject { get; set; }
        public string ContentType { get; set; }
        public string CorrelationId { get; set; }
        public int DeliveryCount { get; set; }
        public string PartitionKey { get; set; }
        public string SessionId { get; set; }
        public string DeadLetterReason { get; set; }
        public string DeadLetterSource { get; set; }
        public string DeadLetterErrorDescription { get; set; }
        public Dictionary<string, object> ApplicationProperties { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, reason, errors);
        }

        public enum FailureReason
        {
            Unknown,
        }
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

            return Result.Success();
        }
    }
}