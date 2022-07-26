using Firepuma.PaymentsService.Abstractions.ValueObjects;

namespace Firepuma.PaymentsService.Abstractions.DTOs.Responses;

public class PayFastOnceOffPaymentResponse
{
    public PayFastPaymentId PaymentId { get; set; }
    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public string Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public PayFastOnceOffPaymentResponse(
        PayFastPaymentId paymentId,
        string emailAddress,
        string nameFirst,
        double immediateAmountInRands,
        string itemName,
        string itemDescription,
        string status,
        DateTime? statusChangedOn)
    {
        PaymentId = paymentId;
        EmailAddress = emailAddress;
        NameFirst = nameFirst;
        ImmediateAmountInRands = immediateAmountInRands;
        ItemName = itemName;
        ItemDescription = itemDescription;
        Status = status;
        StatusChangedOn = statusChangedOn;
    }
}