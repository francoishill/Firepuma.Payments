using Firepuma.Payments.Core.Gateways.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public class GetAvailablePaymentGatewaysResponse
{
    public PaymentGatewayTypeId TypeId { get; set; }
    public string DisplayName { get; set; }
    public PaymentGatewayFeatures Features { get; set; }
}