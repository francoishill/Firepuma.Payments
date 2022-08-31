using System;
using System.Collections.Generic;
using System.Linq;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Gateways;

public static class PaymentGatewaysCollectionExtensions
{
    public static IPaymentGateway GetFromTypeIdOrNull(
        this IEnumerable<IPaymentGateway> gateways,
        PaymentGatewayTypeId gatewayTypeId)
    {
        return gateways.SingleOrDefault(g => string.Equals(g.TypeId.Value, gatewayTypeId.Value, StringComparison.OrdinalIgnoreCase));
    }
}