using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.CommandsAndQueries.CosmosDb.Services;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Infrastructure.CommandHandling.Services;

internal class CommandExecutionPartitionKeyGenerator : ICommandExecutionPartitionKeyGenerator
{
    public string GeneratePartitionKey(CommandExecutionEvent entity)
    {
        return entity.CreatedOn.ToString("yyyy-MM");
    }
}