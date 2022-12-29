using Firepuma.Payments.Domain.Payments.ValueObjects;
using PayFast;

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast.ValueObjects;

public class PayFastNotificationPayload : BasePaymentNotificationPayload
{
    public PayFastNotify PayFastNotify { get; init; } = null!;
}