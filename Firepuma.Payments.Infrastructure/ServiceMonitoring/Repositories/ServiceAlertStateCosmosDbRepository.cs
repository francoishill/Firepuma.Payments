using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.Infrastructure.ServiceMonitoring.Repositories;

public class ServiceAlertStateCosmosDbRepository : CosmosDbRepository<ServiceAlertState>, IServiceAlertStateRepository
{
    public ServiceAlertStateCosmosDbRepository(ILogger logger, Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(ServiceAlertState entity) => entity.AlertType.ToString();
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId);
}