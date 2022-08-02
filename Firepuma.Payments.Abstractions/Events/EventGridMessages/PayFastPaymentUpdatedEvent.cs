﻿using Firepuma.PaymentsService.Abstractions.ValueObjects;

namespace Firepuma.PaymentsService.Abstractions.Events.EventGridMessages;

public class PayFastPaymentUpdatedEvent : IPaymentEventGridMessage
{
    public string CorrelationId { get; set; }
    public string ApplicationId { get; set; }
    public PayFastPaymentId PaymentId { get; set; }
    public PayFastSubscriptionStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}