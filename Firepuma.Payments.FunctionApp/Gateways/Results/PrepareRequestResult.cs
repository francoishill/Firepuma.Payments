using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Gateways.Results;

public class PrepareRequestResult
{
    public PaymentId PaymentId { get; set; }
    public object RequestDto { get; set; }
}