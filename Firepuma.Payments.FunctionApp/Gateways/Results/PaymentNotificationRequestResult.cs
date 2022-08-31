using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Gateways.Results;

public class PaymentNotificationRequestResult
{
    public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }
}