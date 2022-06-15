using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.DTOs.Events;

public class PayFastPaymentItnValidatedEvent
{
    public string ApplicationId { get; set; }
    public string PaymentId { get; set; }
    public PayFastNotify PayFastRequest { get; set; }
    public string IncomingRequestUri { get; set; }

    public PayFastPaymentItnValidatedEvent(
        string applicationId,
        string paymentId,
        PayFastNotify payFastRequest,
        string incomingRequestUri)
    {
        ApplicationId = applicationId;
        PaymentId = paymentId;
        PayFastRequest = payFastRequest;
        IncomingRequestUri = incomingRequestUri;
    }
}