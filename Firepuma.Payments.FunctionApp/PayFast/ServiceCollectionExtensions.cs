using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.TableProviders;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastFeature(
        this IServiceCollection services,
        string validateAndStoreItnBaseUrl)
    {
        services.Configure<PayFastOptions>(opt =>
        {
            opt.ValidateAndStoreItnBaseUrl = validateAndStoreItnBaseUrl;
        });

        services.AddTableProvider("PayFastApplicationConfigs", table => new PayFastApplicationConfigsTableProvider(table));
        services.AddTableProvider("PayFastOnceOffPayments", table => new PayFastOnceOffPaymentsTableProvider(table));
        services.AddTableProvider("PayFastItnTraces", table => new PayFastItnTracesTableProvider(table));

        services.AddScoped<PayFastClientAppConfigProvider>();

        services.AddScoped<IPaymentGateway, PayFastPaymentGateway>();
    }
}