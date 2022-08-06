using System;
using System.Collections.Generic;
using System.Linq;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;

public static class PaymentGatewaysCollectionExtensions
{
    public static IPaymentGateway GetFromTypeIdOrNull(
        this IEnumerable<IPaymentGateway> gateways,
        PaymentGatewayTypeId gatewayTypeId)
    {
        return gateways.SingleOrDefault(g => string.Equals(g.TypeId.Value, gatewayTypeId.Value, StringComparison.OrdinalIgnoreCase));
    }
}