using Firepuma.Payments.Infrastructure.Payments.TableModels;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.Repositories.EntityRepositories;

public class PaymentNotificationTraceCosmosDbRepository : CosmosDbRepository<PaymentNotificationTrace>, IPaymentNotificationTraceRepository
{
    public PaymentNotificationTraceCosmosDbRepository(Container container)
        : base(container)
    {
    }

    public override string GenerateId(PaymentNotificationTrace entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.ApplicationId}";
    public override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}