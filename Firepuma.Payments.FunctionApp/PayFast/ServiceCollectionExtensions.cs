using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.Config;
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

        services.AddTableProvider<PayFastClientAppConfig>("PayFastApplicationConfigs");
        services.AddTableProvider<PayFastOnceOffPayment>("PayFastOnceOffPayments");

        services.AddScoped<PayFastClientAppConfigProvider>();

        services.AddScoped<IPaymentGateway, PayFastPaymentGateway>();
    }
}