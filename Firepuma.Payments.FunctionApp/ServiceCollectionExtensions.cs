using Firepuma.DatabaseRepositories.CosmosDb;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.FunctionApp.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.Config;
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
            CosmosContainerConfiguration.Payments.ContainerProperties.Id,
            (
                logger,
                container,
                _) => new PaymentCosmosDbRepository(logger, container));

        services.AddCosmosDbRepository<
            PaymentNotificationTrace,
            IPaymentNotificationTraceRepository,
            PaymentNotificationTraceCosmosDbRepository>(
            CosmosContainerConfiguration.NotificationTraces.ContainerProperties.Id,
            (
                logger,
                container,
                _) => new PaymentNotificationTraceCosmosDbRepository(logger, container));

        services.AddCosmosDbRepository<
            PaymentApplicationConfig,
            IPaymentApplicationConfigRepository,
            PaymentApplicationConfigCosmosDbRepository>(
            CosmosContainerConfiguration.ApplicationConfigs.ContainerProperties.Id,
            (
                logger,
                container,
                _) => new PaymentApplicationConfigCosmosDbRepository(logger, container));
    }
}