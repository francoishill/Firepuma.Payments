using Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels;
using Firepuma.Payments.Infrastructure.Repositories;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.CommandHandling;

public class CommandExecutionEventCosmosDbRepository : CosmosDbRepository<CommandExecutionEvent>, ICommandExecutionEventRepository
{
    public CommandExecutionEventCosmosDbRepository(Container container)
        : base(container)
    {
    }

    public override string GenerateId(CommandExecutionEvent entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.TypeName}";
    public override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}