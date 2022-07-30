using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.PaymentsService.FunctionApp.PayFast.Commands;
using Firepuma.PaymentsService.FunctionApp.PayFast.DTOs.Events;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.ServiceBusQueueFunctions;

public class ProcessPayFastItnBusMessage
{
    private readonly IMediator _mediator;

    public ProcessPayFastItnBusMessage(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("ProcessPayFastItnBusMessage")]
    public async Task RunAsync(
        ILogger log,
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBus")] ServiceBusReceivedMessage busReceivedMessage,
        CancellationToken cancellationToken)
    {
        var messageJson = busReceivedMessage.Body.ToString();

        log.LogInformation("C# ServiceBus queue trigger function processing message: {QueueMessage}", messageJson);

        var dto = JsonConvert.DeserializeObject<PayFastPaymentItnValidatedEvent>(messageJson);
        if (dto == null)
        {
            log.LogCritical("Dto is null, unable to deserialize messageJson to PayFastPaymentItnValidatedEvent");
            throw new Exception("Dto is null, unable to deserialize messageJson to PayFastPaymentItnValidatedEvent");
        }

        var correlationId = busReceivedMessage.CorrelationId;
        var applicationId = dto.ApplicationId;
        var payFastRequest = dto.PayFastRequest;

        try
        {
            var addTraceCommand = new AddPayFastItnTrace.Command
            {
                ApplicationId = applicationId,
                PayFastRequest = payFastRequest,
                IncomingRequestUri = dto.IncomingRequestUri,
            };

            var addTraceResult = await _mediator.Send(addTraceCommand, cancellationToken);

            if (!addTraceResult.IsSuccessful)
            {
                log.LogError("AddPayFastItnTrace command execution was unsuccessful, reason {Reason}, errors {Errors}", addTraceResult.FailedReason.ToString(), string.Join(", ", addTraceResult.FailedErrors));
            }
        }
        catch (Exception exception)
        {
            log.LogError(exception, "Unable to record PayfastItnTrace, exception was: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);
        }

        log.LogInformation("Payment status is {Status}", payFastRequest.payment_status);

        var command = new UpdatePayFastOnceOffPaymentStatus.Command
        {
            CorrelationId = correlationId,
            ApplicationId = applicationId,
            PaymentId = payFastRequest.m_payment_id,
            PaymentStatus = payFastRequest.payment_status,
            RequestToken = payFastRequest.token,
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            log.LogCritical("UpdatePayFastOnceOffPaymentStatus command execution was unsuccessful, reason {Reason}, errors {Errors}", result.FailedReason.ToString(), string.Join(", ", result.FailedErrors));

            throw new Exception($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }
    }
}