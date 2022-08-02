using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Mappings;
using Firepuma.Payments.FunctionApp.PayFast.Commands;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.PayFast.Api.ServiceBusTriggers;

public class ProcessPaymentBusMessage
{
    private readonly IMediator _mediator;

    public ProcessPaymentBusMessage(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("ProcessPaymentBusMessage")]
    public async Task RunAsync(
        ILogger log,
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBus")] ServiceBusReceivedMessage busReceivedMessage,
        CancellationToken cancellationToken)
    {
        var correlationId = busReceivedMessage.CorrelationId;
        log.LogInformation("C# ServiceBus queue trigger function processing message with Id {Id} and correlationId {CorrelationId}", busReceivedMessage.MessageId, correlationId);

        if (PaymentBusMessageMappings.TryGetPaymentMessage(busReceivedMessage, out var paymentEvent))
        {
            if (paymentEvent is PayFastPaymentItnValidatedMessage itnValidatedMessage)
            {
                var applicationId = itnValidatedMessage.ApplicationId;
                var payFastRequest = itnValidatedMessage.PayFastRequest;

                try
                {
                    var addTraceCommand = new AddPayFastItnTrace.Command
                    {
                        ApplicationId = applicationId,
                        PayFastRequest = payFastRequest,
                        IncomingRequestUri = itnValidatedMessage.IncomingRequestUri,
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
            else
            {
                log.LogCritical("Unsupported message type {Type} for message ID '{Id}'", paymentEvent.GetType().FullName, busReceivedMessage.MessageId);
                throw new Exception($"Unsupported message type {paymentEvent.GetType().FullName} for message ID '{busReceivedMessage.MessageId}'");
            }
        }
        else
        {
            log.LogCritical("Unknown message type for message ID '{Id}'", busReceivedMessage.MessageId);
            throw new Exception($"Unknown message type for message ID '{busReceivedMessage.MessageId}'");
        }
    }
}