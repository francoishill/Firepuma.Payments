using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable UnusedType.Global

namespace Firepuma.Payments.Domain.Payments.CommandFactories;

public class PaymentNotificationValidatedCommandsFactory : ICommandsFactory<ValidatePaymentNotificationCommand.Result>
{
    private readonly ILogger<PaymentNotificationValidatedCommandsFactory> _logger;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentNotificationValidatedCommandsFactory(
        ILogger<PaymentNotificationValidatedCommandsFactory> logger,
        IEnumerable<IPaymentGateway> gateways)
    {
        _logger = logger;
        _gateways = gateways;
    }

    public async Task<IEnumerable<ICommandRequest>> Handle(
        CreateCommandsFromIntegrationEventRequest<ValidatePaymentNotificationCommand.Result> request,
        CancellationToken cancellationToken)
    {
        var eventPayload = request.EventPayload;

        var gatewayTypeId = eventPayload.GatewayTypeId;

        var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            throw new InvalidOperationException($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        await Task.CompletedTask;

        return new ICommandRequest[]
        {
            new UpdatePaymentCommand.Payload
            {
                CorrelationId = eventPayload.CorrelationId,
                GatewayTypeId = gatewayTypeId,
                ApplicationId = eventPayload.ApplicationId,
                PaymentId = eventPayload.PaymentId,
                PaymentStatus = eventPayload.PaymentStatus,
                GatewayInternalTransactionId = eventPayload.GatewayInternalTransactionId,
                PaymentNotificationPayload = eventPayload.PaymentNotificationPayload,
                IncomingRequestUri = eventPayload.IncomingRequestUri,
            },
        };
    }
}