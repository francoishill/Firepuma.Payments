using Firepuma.Payments.Abstractions.ValueObjects;
using PayFast;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;

public class PayFastPaymentItnValidatedMessage : IPaymentBusMessage
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentId PaymentId { get; set; }
    public PayFastNotify PayFastRequest { get; set; }
    public string IncomingRequestUri { get; set; }

    public PayFastPaymentItnValidatedMessage(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        PayFastNotify payFastRequest,
        string incomingRequestUri)
    {
        ApplicationId = applicationId;
        PaymentId = paymentId;
        PayFastRequest = payFastRequest;
        IncomingRequestUri = incomingRequestUri;
    }
}