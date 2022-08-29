using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Core.Events.EventGridMessages;

public class PaymentUpdatedEvent : IPaymentEventGridMessage
{
    public string CorrelationId { get; set; }
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentId PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}