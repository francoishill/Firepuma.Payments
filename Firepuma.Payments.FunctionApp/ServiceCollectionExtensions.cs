using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.Config;
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

        services.AddTableProvider<IPaymentTableEntity>("Payments");
        services.AddTableProvider<PaymentTrace>("PaymentTraces");
        services.AddTableProvider<IPaymentApplicationConfig>("PaymentApplicationConfigs");
    }
}