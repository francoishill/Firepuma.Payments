using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Worker.Payments.Controllers.Responses;

public class GetAvailablePaymentGatewaysResponse
{
    public required PaymentGatewayTypeId TypeId { get; set; }
    public required string DisplayName { get; set; }
    public required PaymentGatewayFeatures Features { get; set; }
}