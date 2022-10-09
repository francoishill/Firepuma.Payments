using Firepuma.DatabaseRepositories.CosmosDb;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.ServiceMonitoring.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.ServiceMonitoring;

public static class ServiceCollectionExtensions
{
    public static void AddServiceMonitoring(
        this IServiceCollection services)
    {
        services.AddCosmosDbRepository<
            DeadLetteredMessage,
            IDeadLetteredMessageRepository,
            DeadLetteredMessageCosmosDbRepository>(
            CosmosContainerConfiguration.DeadLetteredMessages.ContainerProperties.Id,
            (
                logger,
                container,
                _) => new DeadLetteredMessageCosmosDbRepository(logger, container));

        services.AddCosmosDbRepository<
            ServiceAlertState,
            IServiceAlertStateRepository,
            ServiceAlertStateCosmosDbRepository>(
            CosmosContainerConfiguration.ServiceAlertStates.ContainerProperties.Id,
            (
                logger,
                container,
                _) => new ServiceAlertStateCosmosDbRepository(logger, container));
    }
}