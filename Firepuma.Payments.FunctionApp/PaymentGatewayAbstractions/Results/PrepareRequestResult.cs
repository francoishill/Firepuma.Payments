using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;

public class PrepareRequestResult
{
    public PaymentId PaymentId { get; set; }
    public object RequestDto { get; set; }
}