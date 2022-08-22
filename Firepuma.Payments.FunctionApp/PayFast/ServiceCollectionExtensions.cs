using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastFeature(
        this IServiceCollection services)
    {
        services.AddScoped<IPaymentGateway, PayFastPaymentGateway>();
    }
}