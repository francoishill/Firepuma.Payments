using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
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
            CosmosContainerNames.DEAD_LETTERED_MESSAGES,
            (logger, container) => new DeadLetteredMessageCosmosDbRepository(logger, container));
    }
}