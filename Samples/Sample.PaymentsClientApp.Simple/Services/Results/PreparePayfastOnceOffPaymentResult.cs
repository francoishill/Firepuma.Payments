namespace Sample.PaymentsClientApp.Simple.Services.Results;

public class PreparePayfastOnceOffPaymentResult
{
    public Uri RedirectUrl { get; set; }
    public string PaymentId { get; set; }

    public PreparePayfastOnceOffPaymentResult(Uri redirectUrl, string paymentId)
    {
        RedirectUrl = redirectUrl;
        PaymentId = paymentId;
    }
}