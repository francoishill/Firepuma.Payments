using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public class PreparePayFastOnceOffPaymentResponse
{
    public PaymentId PaymentId { get; set; }
    public string RedirectUrl { get; set; }

    public PreparePayFastOnceOffPaymentResponse(PaymentId paymentId, Uri redirectUrl)
    {
        PaymentId = paymentId;
        RedirectUrl = redirectUrl.AbsoluteUri;
    }
}