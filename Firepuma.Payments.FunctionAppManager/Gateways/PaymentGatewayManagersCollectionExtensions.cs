using System;
using System.Collections.Generic;
using System.Linq;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.FunctionAppManager.Gateways;

public static class PaymentGatewayManagersCollectionExtensions
{
    public static IPaymentGatewayManager GetFromTypeIdOrNull(
        this IEnumerable<IPaymentGatewayManager> gatewayManagers,
        PaymentGatewayTypeId gatewayTypeId)
    {
        return gatewayManagers.SingleOrDefault(g => string.Equals(g.TypeId.Value, gatewayTypeId.Value, StringComparison.OrdinalIgnoreCase));
    }
}