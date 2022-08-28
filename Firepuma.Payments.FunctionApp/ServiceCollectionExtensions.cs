using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.Payments.TableModels;
using Firepuma.Payments.Implementations.Repositories;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
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

        services.AddCosmosDbRepository<
            PaymentEntity,
            IPaymentRepository,
            PaymentCosmosDbRepository>(
            CosmosContainerNames.PAYMENTS,
            container => new PaymentCosmosDbRepository(container));

        services.AddCosmosDbRepository<
            PaymentNotificationTrace,
            IPaymentNotificationTraceRepository,
            PaymentNotificationTraceCosmosDbRepository>(
            CosmosContainerNames.NOTIFICATION_TRACES,
            container => new PaymentNotificationTraceCosmosDbRepository(container));

        services.AddCosmosDbRepository<
            PaymentApplicationConfig,
            IPaymentApplicationConfigRepository,
            PaymentApplicationConfigCosmosDbRepository>(
            CosmosContainerNames.APPLICATION_CONFIGS,
            container => new PaymentApplicationConfigCosmosDbRepository(container));
    }
}