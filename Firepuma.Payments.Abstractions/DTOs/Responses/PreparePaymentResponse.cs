using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Abstractions.DTOs.Responses;

public class PreparePaymentResponse
{
    public PaymentId PaymentId { get; set; }
    public string RedirectUrl { get; set; }

    public PreparePaymentResponse(PaymentId paymentId, Uri redirectUrl)
    {
        PaymentId = paymentId;
        RedirectUrl = redirectUrl.AbsoluteUri;
    }
}