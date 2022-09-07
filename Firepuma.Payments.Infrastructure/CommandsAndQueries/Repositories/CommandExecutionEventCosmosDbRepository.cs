using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.CommandsAndQueries.Repositories;

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