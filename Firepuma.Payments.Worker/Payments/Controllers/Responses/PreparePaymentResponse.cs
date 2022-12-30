using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Worker.Payments.Controllers.Responses;

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