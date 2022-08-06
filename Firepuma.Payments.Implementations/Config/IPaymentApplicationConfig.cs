using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Implementations.Config;

public interface IPaymentApplicationConfig
{
    public PaymentGatewayTypeId GatewayTypeId { get; }
    public ClientApplicationId ApplicationId { get; }
    public string ApplicationSecret { get; set; }
}