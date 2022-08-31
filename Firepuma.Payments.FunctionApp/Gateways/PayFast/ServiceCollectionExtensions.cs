using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp.Gateways.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastFeature(
        this IServiceCollection services)
    {
        services.AddScoped<IPaymentGateway, PayFastPaymentGateway>();
    }
}