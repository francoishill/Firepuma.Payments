using Firepuma.Payments.Implementations.CommandHandling.TableModels;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public class CommandExecutionEventCosmosDbRepository : CosmosDbRepository<CommandExecutionEvent>, ICommandExecutionEventRepository
{
    public CommandExecutionEventCosmosDbRepository(Container container)
        : base(container)
    {
    }

    public override string GenerateId(CommandExecutionEvent entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.TypeName}";
    public override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}