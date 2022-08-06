using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Firepuma.Payments.FunctionApp.TableProviders;
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