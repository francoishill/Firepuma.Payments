using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionAppManager.Gateways.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastManagerFeature(
        this IServiceCollection services)
    {
        services.AddScoped<IPaymentGatewayManager, PayFastPaymentGatewayManager>();
    }
}