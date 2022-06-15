using Firepuma.PaymentsService.Abstractions.ValueObjects;

namespace Firepuma.PaymentsService.Abstractions.DTOs.Events;

public class PayFastPaymentUpdatedEvent
{
    public PayFastPaymentId PaymentId { get; set; }
    public PayFastSubscriptionStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
    public string CorrelationId { get; set; }

    public PayFastPaymentUpdatedEvent(
        PayFastPaymentId paymentId,
        PayFastSubscriptionStatus status,
        DateTime? statusChangedOn,
        string correlationId)
    {
        PaymentId = paymentId;
        Status = status;
        StatusChangedOn = statusChangedOn;
        CorrelationId = correlationId;
    }
}