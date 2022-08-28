using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Implementations.Repositories;

public interface ICosmosContainerContext<in T>
{
    string ContainerName { get; }
    string GenerateId(T entity);
    PartitionKey ResolvePartitionKey(string entityId);
}