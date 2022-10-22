using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Email.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.Email.Client.Services;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;
using Firepuma.Payments.FunctionAppManager.ValueObjects;
using FluentValidation;
using MediatR;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class AlertNewDeadLetteredMessages
{
    public class Command : BaseCommand<Result>
    {
        public string AlertRecipientEmail { get; init; } = null!;
        public string EmailClientApplicationId { get; init; } = null!;
        public ServiceAlertState? LastAlertState { get; init; }
        public int CurrentDeadMessageCount { get; init; }
        public int PreviousDeadMessageCount { get; init; }
    }

    public class Result
    {
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AlertRecipientEmail).NotEmpty();
            RuleFor(x => x.EmailClientApplicationId).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IServiceAlertStateRepository _serviceAlertStateRepository;
        private readonly IEmailEnqueuingClient _emailEnqueuingClient;

        public Handler(
            IServiceAlertStateRepository serviceAlertStateRepository,
            IEmailEnqueuingClient emailEnqueuingClient)
        {
            _serviceAlertStateRepository = serviceAlertStateRepository;
            _emailEnqueuingClient = emailEnqueuingClient;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var lastAlertState = command.LastAlertState;
            var currentDeadMessageCount = command.CurrentDeadMessageCount;
            var previousDeadMessageCount = command.PreviousDeadMessageCount;
            var alertRecipientEmail = command.AlertRecipientEmail;

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

            return new Result();
        }
    }
}