using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.MessageBus.BusMessages;

public class PayFastPaymentItnValidatedMessage : IPaymentBusMessage
{
    public string ApplicationId { get; set; }
    public string PaymentId { get; set; }
    public PayFastNotify PayFastRequest { get; set; }
    public string IncomingRequestUri { get; set; }

    public PayFastPaymentItnValidatedMessage(
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