using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.FunctionAppManager.Commands;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
using Firepuma.Payments.FunctionAppManager.Queries;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Firepuma.Payments.FunctionAppManager.Api.TimerTriggers;

public class TriggerAlertNewDeadLetteredMessages
{
    private readonly IOptions<AdditionalEmailServiceClientOptions> _emailOptions;
    private readonly IMediator _mediator;

    public TriggerAlertNewDeadLetteredMessages(
        IOptions<AdditionalEmailServiceClientOptions> emailOptions,
        IMediator mediator)
    {
        _emailOptions = emailOptions;
        _mediator = mediator;
    }

    [FunctionName("TriggerAlertNewDeadLetteredMessages")]
    public async Task RunAsync(
        [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# Timer trigger function executed at: {Time}", DateTime.UtcNow.ToString("O"));

        var query = new CheckDeadMessageCountsQuery.Query
        {
            NowDateTimeOffset = DateTimeOffset.UtcNow,
        };

        var queryResult = await _mediator.Send(query, cancellationToken);

        if (!queryResult.IsDue)
        {
            log.LogInformation("Nothing to do now, next check time is {Time}", queryResult.LastAlertState.NextCheckTime.ToString("O"));
            return;
        }

        if (!queryResult.CountIsDifferent)
        {
            log.LogInformation("There are no new dead lettered messages, previously and current alerted count of dead letter messages is {Count}", queryResult.CurrentDeadMessageCount);
            return;
        }

        var alertCommand = new AlertNewDeadLetteredMessages.Command
        {
            AlertRecipientEmail = _emailOptions.Value.AlertRecipientEmail,
            EmailClientApplicationId = "payments-service",
            LastAlertState = queryResult.LastAlertState,
            CurrentDeadMessageCount = queryResult.CurrentDeadMessageCount,
            PreviousDeadMessageCount = queryResult.PreviousDeadMessageCount,
        };

        try
        {
            await _mediator.Send(alertCommand, cancellationToken);
        }
        catch (CommandException commandException)
        {
            log.LogCritical(
                "Failed to alert dead lettered message, status {Status}, errors {Errors}",
                commandException.StatusCode.ToString(), JsonConvert.SerializeObject(commandException.Errors));

            throw;
        }
    }
}