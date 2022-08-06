using Firepuma.Payments.Abstractions.ValueObjects;

namespace Sample.PaymentsClientApp.Simple.Services.Results;

public class PreparePayfastOnceOffPaymentResult
{
    public Uri RedirectUrl { get; set; }
    public PaymentId PaymentId { get; set; }

    public PreparePayfastOnceOffPaymentResult(Uri redirectUrl, PaymentId paymentId)
    {
        RedirectUrl = redirectUrl;
        PaymentId = paymentId;
    }
}