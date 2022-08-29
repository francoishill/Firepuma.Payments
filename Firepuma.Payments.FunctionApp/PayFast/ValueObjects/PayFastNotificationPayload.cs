using Firepuma.Payments.Core.ValueObjects;
using PayFast;

namespace Firepuma.Payments.FunctionApp.PayFast.ValueObjects;

public class PayFastNotificationPayload : BasePaymentNotificationPayload
{
    public PayFastNotify PayFastNotify { get; init; }
}