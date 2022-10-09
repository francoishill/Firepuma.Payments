﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.FunctionApp.Commands;
using Firepuma.Payments.FunctionApp.Gateways;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Mappings;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

                try
                {
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

                    await _mediator.Send(updateCommand, cancellationToken);
                }
                catch (CommandException commandException)
                {
                    log.LogCritical(
                        "UpdatePayment command execution was unsuccessful, status {Status}, errors {Errors}",
                        commandException.StatusCode.ToString(), JsonConvert.SerializeObject(commandException.Errors));

                    throw;
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