using Firepuma.Payments.Implementations.TableProviders;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsFeature(this IServiceCollection services)
    {
        services.AddTableProvider("Payments", table => new PaymentsTableProvider(table));
        services.AddTableProvider("PaymentApplicationConfigs", table => new ApplicationConfigsTableProvider(table));
    }
}