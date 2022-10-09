using Firepuma.DatabaseRepositories.CosmosDb;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.PaymentAppConfiguration.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionAppManager;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsManagementFeature(
        this IServiceCollection services)
    {
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