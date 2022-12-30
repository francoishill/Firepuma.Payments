using Firepuma.DatabaseRepositories.MongoDb;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Config;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.Services;
using Firepuma.Payments.Infrastructure.Payments.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArgumentsStyleNamedExpression

namespace Firepuma.Payments.Infrastructure.Payments;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsFeature(
        this IServiceCollection services,
        IConfigurationSection paymentsConfigSection,
        string appConfigurationsCollectionName,
        string paymentsCollectionName,
        string notificationTracesCollectionName)
    {
        if (paymentsConfigSection == null) throw new ArgumentNullException(nameof(paymentsConfigSection));
        if (string.IsNullOrWhiteSpace(paymentsCollectionName)) throw new ArgumentNullException(nameof(paymentsCollectionName));
        if (string.IsNullOrWhiteSpace(notificationTracesCollectionName)) throw new ArgumentNullException(nameof(notificationTracesCollectionName));

        services.AddOptions<PaymentGeneralOptions>().Bind(paymentsConfigSection).ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<IApplicationConfigProvider, CachedApplicationConfigProvider>();

        services.AddMongoDbRepository<
            PaymentApplicationConfig,
            IPaymentApplicationConfigRepository,
            PaymentApplicationConfigMongoDbRepository>(
            appConfigurationsCollectionName,
            (logger, collection, _) => new PaymentApplicationConfigMongoDbRepository(logger, collection),
            indexesFactory: PaymentApplicationConfig.GetSchemaIndexes);

        services.AddMongoDbRepository<
            PaymentEntity,
            IPaymentRepository,
            PaymentMongoDbRepository>(
            paymentsCollectionName,
            (logger, collection, _) => new PaymentMongoDbRepository(logger, collection),
            indexesFactory: PaymentEntity.GetSchemaIndexes);

        services.AddMongoDbRepository<
            PaymentNotificationTrace,
            IPaymentNotificationTraceRepository,
            PaymentNotificationTraceMongoDbRepository>(
            notificationTracesCollectionName,
            (logger, collection, _) => new PaymentNotificationTraceMongoDbRepository(logger, collection),
            indexesFactory: PaymentNotificationTrace.GetSchemaIndexes);
    }
}