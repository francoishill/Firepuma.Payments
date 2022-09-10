using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.Infrastructure.CommandsAndQueries.Repositories;

public class AuthorizationFailureEventCosmosDbRepository : CosmosDbRepository<AuthorizationFailureEvent>, IAuthorizationFailureEventRepository
{
    public AuthorizationFailureEventCosmosDbRepository(
        ILogger<AuthorizationFailureEventCosmosDbRepository> logger,
        Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(AuthorizationFailureEvent entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.ActionTypeName}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}