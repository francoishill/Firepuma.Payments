using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.CommandsAndQueries.CosmosDb.Services;

namespace Firepuma.Payments.Infrastructure.CommandHandling.Services;

public class AuthorizationFailurePartitionKeyGenerator : IAuthorizationFailurePartitionKeyGenerator
{
    public string GeneratePartitionKey(AuthorizationFailureEvent entity)
    {
        return entity.CreatedOn.ToString("yyyy-MM");
    }
}