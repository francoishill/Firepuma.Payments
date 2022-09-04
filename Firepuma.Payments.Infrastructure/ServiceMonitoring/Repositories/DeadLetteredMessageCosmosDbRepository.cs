using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.Infrastructure.ServiceMonitoring.Repositories;

public class DeadLetteredMessageCosmosDbRepository : CosmosDbRepository<DeadLetteredMessage>, IDeadLetteredMessageRepository
{
    public DeadLetteredMessageCosmosDbRepository(
        ILogger logger,
        Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(DeadLetteredMessage entity) => $"{entity.MessageId}-{Guid.NewGuid().ToString()}:{entity.EnqueuedYearAndMonth}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}