using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Email.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.Email.Client.Services;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Specifications;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Firepuma.Payments.FunctionAppManager.Api.TimerTriggers;

public class AlertNewDeadLetteredMessages
{
    private readonly IOptions<AdditionalEmailServiceClientOptions> _emailOptions;
    private readonly IServiceAlertStateRepository _serviceAlertStateRepository;
    private readonly IDeadLetteredMessageRepository _deadLetteredMessageRepository;
    private readonly IEmailEnqueuingClient _emailEnqueuingClient;

    public AlertNewDeadLetteredMessages(
        IOptions<AdditionalEmailServiceClientOptions> emailOptions,
        IServiceAlertStateRepository serviceAlertStateRepository,
        IDeadLetteredMessageRepository deadLetteredMessageRepository,
        IEmailEnqueuingClient emailEnqueuingClient)
    {
        _emailOptions = emailOptions;
        _serviceAlertStateRepository = serviceAlertStateRepository;
        _deadLetteredMessageRepository = deadLetteredMessageRepository;
        _emailEnqueuingClient = emailEnqueuingClient;
    }

    [FunctionName("AlertNewDeadLetteredMessages")]
    public async Task RunAsync(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# Timer trigger function executed at: {Time}", DateTime.UtcNow.ToString("O"));

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
                log.LogError("Unable to cast lastAlertState context to NewDeadLetteredMessagesExtraValues, error: {Error}", castError);
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
                        ApplicationId = "payments-service",
                        Subject = $"Payments Service has {currentDeadMessageCount} dead lettered messages",
                        ToEmail = _emailOptions.Value.AlertRecipientEmail,
                        TextBody = $"There were previously {previousDeadMessageCount} dead lettered messages and there are now {currentDeadMessageCount}.",
                        HtmlBody = $"There were previously {previousDeadMessageCount} dead lettered messages and there are now <strong>{currentDeadMessageCount}</strong>.",
                    },
                    cancellationToken);

                await _serviceAlertStateRepository.UpsertItemAsync(lastAlertState, cancellationToken: cancellationToken);
            }
            else
            {
                log.LogInformation("There are no new dead lettered messages, previously and current alerted count of dead letter messages is {Count}", currentDeadMessageCount);
            }
        }
        else
        {
            log.LogInformation(
                "Nothing to do now, next check time is {Time}",
                lastAlertState.NextCheckTime.ToString("O"));
        }
    }

    private class NewDeadLetteredMessagesExtraValues
    {
        public int TotalDeadLetteredMessages { get; set; }
    }
}