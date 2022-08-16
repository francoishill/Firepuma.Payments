using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;

public class ValidatePaymentNotificationResult
{
    public PaymentId PaymentId { get; set; }
}