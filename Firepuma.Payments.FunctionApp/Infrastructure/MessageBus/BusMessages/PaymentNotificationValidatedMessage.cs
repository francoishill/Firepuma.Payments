using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;

public class PaymentNotificationValidatedMessage : IPaymentBusMessage
{
    public PaymentGatewayTypeId GatewayTypeId { get; init; }
    public ClientApplicationId ApplicationId { get; init; }
    public PaymentId PaymentId { get; init; }
    public string GatewayInternalTransactionId { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }
    public string IncomingRequestUri { get; init; }
}