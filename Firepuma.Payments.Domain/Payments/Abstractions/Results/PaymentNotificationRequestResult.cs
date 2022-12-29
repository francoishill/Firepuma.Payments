using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Abstractions.Results;

public class PaymentNotificationRequestResult
{
    public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; } = null!;
}