using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.Core.Infrastructure.Events.EventGridMessages;

public class PaymentUpdatedEvent : IPaymentEventGridMessage
{
    public string CorrelationId { get; set; } = null!;
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentId PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}