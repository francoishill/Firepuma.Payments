using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Core.DTOs.Responses;

public class PayFastOnceOffPaymentResponse
{
    public PaymentId PaymentId { get; set; }
    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public string Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}