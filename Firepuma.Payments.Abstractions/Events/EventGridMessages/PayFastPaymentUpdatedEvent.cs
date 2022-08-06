﻿using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Abstractions.Events.EventGridMessages;

public class PayFastPaymentUpdatedEvent : IPaymentEventGridMessage
{
    public string CorrelationId { get; set; }
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentId PaymentId { get; set; }
    public PayFastSubscriptionStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}