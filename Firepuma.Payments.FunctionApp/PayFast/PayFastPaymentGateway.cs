using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;

namespace Firepuma.Payments.FunctionApp.PayFast;

public class PayFastPaymentGateway : IPaymentGateway
{
    public PaymentGatewayTypeId TypeId => new("PayFast");
    public string DisplayName => "PayFast";

    public PaymentGatewayFeatures Features => new()
    {
        PreparePaymentRedirectUrl = true,
    };
}