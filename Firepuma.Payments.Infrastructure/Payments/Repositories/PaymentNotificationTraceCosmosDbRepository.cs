using Firepuma.DatabaseRepositories.CosmosDb.Repositories;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentNotificationTraceCosmosDbRepository : CosmosDbRepository<PaymentNotificationTrace>, IPaymentNotificationTraceRepository
{
    public PaymentNotificationTraceCosmosDbRepository(
        ILogger<PaymentNotificationTraceCosmosDbRepository> logger,
        Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(PaymentNotificationTrace entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.ApplicationId}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}