using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionAppManager;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsManagementFeature(
        this IServiceCollection services)
    {
        services.AddTableProvider<BasePaymentApplicationConfig>("PaymentApplicationConfigs");
    }
}