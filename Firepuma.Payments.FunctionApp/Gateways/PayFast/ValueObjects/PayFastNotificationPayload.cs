using Firepuma.Payments.Core.Payments.ValueObjects;
using PayFast;

namespace Firepuma.Payments.FunctionApp.Gateways.PayFast.ValueObjects;

public class PayFastNotificationPayload : BasePaymentNotificationPayload
{
    public PayFastNotify PayFastNotify { get; init; } = null!;
}