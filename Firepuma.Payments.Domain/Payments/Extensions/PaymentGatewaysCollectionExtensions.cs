using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Extensions;

public static class PaymentGatewaysCollectionExtensions
{
    public static IPaymentGateway? GetFromTypeIdOrNull(
        this IEnumerable<IPaymentGateway> gateways,
        PaymentGatewayTypeId gatewayTypeId)
    {
        return gateways.SingleOrDefault(g => string.Equals(g.TypeId.Value, gatewayTypeId.Value, StringComparison.OrdinalIgnoreCase));
    }

    public static IPaymentGatewayManager? GetFromTypeIdOrNull(
        this IEnumerable<IPaymentGatewayManager> gatewayManagers,
        PaymentGatewayTypeId gatewayTypeId)
    {
        return gatewayManagers.SingleOrDefault(g => string.Equals(g.TypeId.Value, gatewayTypeId.Value, StringComparison.OrdinalIgnoreCase));
    }
}