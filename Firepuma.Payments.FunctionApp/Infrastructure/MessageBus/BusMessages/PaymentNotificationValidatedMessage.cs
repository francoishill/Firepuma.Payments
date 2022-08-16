using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;

public class PaymentNotificationValidatedMessage : IPaymentBusMessage
{
    public PaymentGatewayTypeId GatewayTypeId { get; init; }
    public ClientApplicationId ApplicationId { get; init; }
    public PaymentId PaymentId { get; init; }
    public object PaymentNotificationPayload { get; init; }
    public string IncomingRequestUri { get; init; }
}