﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Commands;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Mappings;
using Firepuma.Payments.FunctionApp.PayFast.Commands;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.Api.ServiceBusTriggers;

public class ProcessPaymentBusMessage
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public ProcessPaymentBusMessage(
        IMediator mediator,
        IEnumerable<IPaymentGateway> gateways)
    {
        _mediator = mediator;
        _gateways = gateways;
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
            if (paymentEvent is PaymentNotificationValidatedMessage paymentNotificationValidatedMessage)
            {
                var gatewayTypeId = paymentNotificationValidatedMessage.GatewayTypeId;

                var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

                if (gateway == null)
                {
                    log.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
                    throw new InvalidOperationException($"The payment gateway type '{gatewayTypeId}' is not supported");
                }

                var updateCommand = new UpdatePayment.Command
                {
                    CorrelationId = correlationId,
                    GatewayTypeId = gatewayTypeId,
                    ApplicationId = paymentNotificationValidatedMessage.ApplicationId,
                    PaymentId = paymentNotificationValidatedMessage.PaymentId,
                    PaymentStatus = paymentNotificationValidatedMessage.PaymentStatus,
                    GatewayInternalTransactionId = paymentNotificationValidatedMessage.GatewayInternalTransactionId,
                    PaymentNotificationPayload = paymentNotificationValidatedMessage.PaymentNotificationPayload,
                    IncomingRequestUri = paymentNotificationValidatedMessage.IncomingRequestUri,
                };

                var updateResult = await _mediator.Send(updateCommand, cancellationToken);
                if (!updateResult.IsSuccessful)
                {
                    log.LogCritical("UpdatePayFastOnceOffPaymentStatus command execution was unsuccessful, reason {Reason}, errors {Errors}", updateResult.FailedReason.ToString(), string.Join(", ", updateResult.FailedErrors));

                    throw new Exception($"{updateResult.FailedReason.ToString()}, {string.Join(", ", updateResult.FailedErrors)}");
                }
            }
            else if (paymentEvent is PayFastPaymentItnValidatedMessage itnValidatedMessage)
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
                    PaymentId = new PaymentId(payFastRequest.m_payment_id),
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