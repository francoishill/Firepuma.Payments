using Firepuma.Payments.Domain.Payments.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastFeature(
        this IServiceCollection services)
    {
        services.AddTransient<IPaymentGateway, PayFastPaymentGateway>();

        services.AddTransient<IPaymentGatewayManager, PayFastPaymentGatewayManager>();
    }
}