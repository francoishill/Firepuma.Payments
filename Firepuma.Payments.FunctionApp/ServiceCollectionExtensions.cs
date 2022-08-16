using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.Implementations.TableProviders;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsFeature(
        this IServiceCollection services,
        string validateAndStorePaymentNotificationBaseUrl)
    {
        services.Configure<PaymentGeneralOptions>(opt =>
        {
            opt.ValidateAndStorePaymentNotificationBaseUrl = validateAndStorePaymentNotificationBaseUrl;
        });

        services.AddTableProvider("Payments", table => new PaymentsTableProvider(table));
        services.AddTableProvider("PaymentApplicationConfigs", table => new ApplicationConfigsTableProvider(table));
    }
}