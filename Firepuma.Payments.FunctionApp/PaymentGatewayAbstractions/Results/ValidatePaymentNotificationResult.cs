using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;

public class ValidatePaymentNotificationResult
{
    public PaymentId PaymentId { get; private init; }
    public string GatewayInternalTransactionId { get; private init; }
    public PaymentStatus PaymentStatus { get; set; }

    public ValidatePaymentNotificationResult(
        PaymentId paymentId,
        string gatewayInternalTransactionId,
        PaymentStatus paymentStatus)
    {
        PaymentId = paymentId;
        GatewayInternalTransactionId = gatewayInternalTransactionId;
        PaymentStatus = paymentStatus;
    }
}