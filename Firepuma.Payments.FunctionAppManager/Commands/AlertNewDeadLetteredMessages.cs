using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Email.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.Email.Client.Services;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Specifications;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class AlertNewDeadLetteredMessages
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string AlertRecipientEmail { get; set; }
        public string EmailClientApplicationId { get; set; }
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
        private readonly IServiceAlertStateRepository _serviceAlertStateRepository;
        private readonly IDeadLetteredMessageRepository _deadLetteredMessageRepository;
        private readonly IEmailEnqueuingClient _emailEnqueuingClient;

        public Handler(
            ILogger<Handler> logger,
            IServiceAlertStateRepository serviceAlertStateRepository,
            IDeadLetteredMessageRepository deadLetteredMessageRepository,
            IEmailEnqueuingClient emailEnqueuingClient)
        {
            _logger = logger;
            _serviceAlertStateRepository = serviceAlertStateRepository;
            _deadLetteredMessageRepository = deadLetteredMessageRepository;
            _emailEnqueuingClient = emailEnqueuingClient;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var alertRecipientEmail = command.AlertRecipientEmail;

            var lastAlertState = await _serviceAlertStateRepository.GetItemOrDefaultAsync(ServiceAlertType.NewDeadLetteredMessages.ToString(), cancellationToken);

            if (lastAlertState == null || lastAlertState.NextCheckTime <= DateTimeOffset.UtcNow)
            {
                int previousDeadMessageCount;

                if (lastAlertState == null)
                {
                    previousDeadMessageCount = 0;
                }
                else if (!lastAlertState.TryCastAlertContextToType<NewDeadLetteredMessagesExtraValues>(out var alertContext, out var castError))
                {
                    previousDeadMessageCount = 0;
                    _logger.LogError("Unable to cast lastAlertState context to NewDeadLetteredMessagesExtraValues, error: {Error}", castError);
                }
                else
                {
                    previousDeadMessageCount = alertContext.TotalDeadLetteredMessages;
                }

                var currentDeadMessageCount = await _deadLetteredMessageRepository.GetItemsCountAsync(new AllDeadLetteredMessagesSpecification(), cancellationToken);

                if (currentDeadMessageCount != previousDeadMessageCount)
                {
                    if (lastAlertState == null)
                    {
                        lastAlertState = new ServiceAlertState
                        {
                            AlertType = ServiceAlertType.NewDeadLetteredMessages,
                        };
                    }

                    lastAlertState.NextCheckTime = DateTimeOffset.UtcNow.AddMinutes(45);

                    lastAlertState.SetAlertContext(new NewDeadLetteredMessagesExtraValues
                    {
                        TotalDeadLetteredMessages = currentDeadMessageCount,
                    });

                    await _emailEnqueuingClient.EnqueueEmail(
                        new SendEmailRequestDto
                        {
                            ApplicationId = command.EmailClientApplicationId,
                            Subject = $"Payments Service has {currentDeadMessageCount} dead lettered messages",
                            ToEmail = alertRecipientEmail,
                            TextBody = $"There were previously {previousDeadMessageCount} dead lettered messages and there are now {currentDeadMessageCount}.",
                            HtmlBody = $"There were previously {previousDeadMessageCount} dead lettered messages and there are now <strong>{currentDeadMessageCount}</strong>.",
                        },
                        cancellationToken);

                    await _serviceAlertStateRepository.UpsertItemAsync(lastAlertState, cancellationToken: cancellationToken);
                }
                else
                {
                    _logger.LogInformation("There are no new dead lettered messages, previously and current alerted count of dead letter messages is {Count}", currentDeadMessageCount);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Nothing to do now, next check time is {Time}",
                    lastAlertState.NextCheckTime.ToString("O"));
            }

            return Result.Success();
        }

        private class NewDeadLetteredMessagesExtraValues
        {
            public int TotalDeadLetteredMessages { get; set; }
        }
    }
}