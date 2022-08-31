using Firepuma.Payments.Core.Infrastructure.CommandHandling.Entities;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.CommandHandling.Repositories;

public class CommandExecutionEventCosmosDbRepository : CosmosDbRepository<CommandExecutionEvent>, ICommandExecutionEventRepository
{
    public CommandExecutionEventCosmosDbRepository(
        ILogger<CommandExecutionEventCosmosDbRepository> logger,
        Container container)
        : base(logger, container)
    {
    }

    protected override string GenerateId(CommandExecutionEvent entity) => $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}:{entity.TypeName}";
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);
}