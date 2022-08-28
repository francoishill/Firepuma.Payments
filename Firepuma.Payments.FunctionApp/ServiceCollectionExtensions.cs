using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.Azure.Cosmos;
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
        services.AddTableProvider<PaymentNotificationTrace>("PaymentNotificationTraces");

        services.AddSingleton<IPaymentApplicationConfigRepository, PaymentApplicationConfigCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("ApplicationConfigs");
            return new PaymentApplicationConfigCosmosDbRepository(container);
        });
    }
}