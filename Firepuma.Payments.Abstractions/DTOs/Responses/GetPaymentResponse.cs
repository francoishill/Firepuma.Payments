namespace Firepuma.Payments.Abstractions.DTOs.Responses;

public class GetPaymentResponse
{
    public string PaymentId { get; set; }
    public string GatewayTypeId { get; set; }
    public object PaymentEntity { get; set; }
}