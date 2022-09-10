using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Firepuma.Payments.FunctionAppManager.Commands;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
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

        var alertCommand = new AlertNewDeadLetteredMessages.Command
        {
            AlertRecipientEmail = _emailOptions.Value.AlertRecipientEmail,
            EmailClientApplicationId = "payments-service",
        };

        try
        {
            await _mediator.Send(alertCommand, cancellationToken);
        }
        catch (WrappedRequestException wrappedRequestException)
        {
            log.LogCritical(
                "Failed to alert dead lettered message, status {Status}, errors {Errors}",
                wrappedRequestException.StatusCode.ToString(), JsonConvert.SerializeObject(wrappedRequestException.Errors));

            throw;
        }
    }
}