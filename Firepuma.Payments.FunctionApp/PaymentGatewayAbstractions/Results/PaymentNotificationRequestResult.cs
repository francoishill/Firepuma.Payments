using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;

public class PaymentNotificationRequestResult
{
    public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }
}