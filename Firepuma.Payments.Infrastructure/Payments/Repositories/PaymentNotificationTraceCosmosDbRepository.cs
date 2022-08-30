using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentNotificationTraceCosmosDbRepository : CosmosDbRepository<PaymentNotificationTrace>, IPaymentNotificationTraceRepository
{
    public PaymentNotificationTraceCosmosDbRepository(Container container)
        : base(container)
    {
    }

    protected override string GenerateId(PaymentNotificationTrace entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.ApplicationId}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}