using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
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

        services.AddSingleton<IPaymentRepository, PaymentCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("Payments");
            return new PaymentCosmosDbRepository(container);
        });

        services.AddSingleton<IPaymentNotificationTraceRepository, PaymentNotificationTraceCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("NotificationTraces");
            return new PaymentNotificationTraceCosmosDbRepository(container);
        });

        services.AddSingleton<IPaymentApplicationConfigRepository, PaymentApplicationConfigCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("ApplicationConfigs");
            return new PaymentApplicationConfigCosmosDbRepository(container);
        });
    }
}