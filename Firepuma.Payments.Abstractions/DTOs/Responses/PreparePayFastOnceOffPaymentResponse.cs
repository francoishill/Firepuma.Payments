namespace Firepuma.Payments.Abstractions.DTOs.Responses;

public class PreparePayFastOnceOffPaymentResponse
{
    public string PaymentId { get; set; }
    public string RedirectUrl { get; set; }

    public PreparePayFastOnceOffPaymentResponse(string paymentId, Uri redirectUrl)
    {
        PaymentId = paymentId;
        RedirectUrl = redirectUrl.AbsoluteUri;
    }
}