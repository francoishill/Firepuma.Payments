using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;

public class PaymentNotificationValidatedMessage : IPaymentBusMessage
{
    public PaymentGatewayTypeId GatewayTypeId { get; init; }
    public ClientApplicationId ApplicationId { get; init; }
    public PaymentId PaymentId { get; init; }
    public string GatewayInternalTransactionId { get; init; } = null!;
    public PaymentStatus PaymentStatus { get; init; }
    public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; } = null!;
    public string IncomingRequestUri { get; init; } = null!;
}