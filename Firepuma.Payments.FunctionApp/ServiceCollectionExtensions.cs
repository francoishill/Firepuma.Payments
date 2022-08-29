using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.FunctionApp.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Firepuma.Payments.Infrastructure.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Infrastructure.Payments.Repositories;
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