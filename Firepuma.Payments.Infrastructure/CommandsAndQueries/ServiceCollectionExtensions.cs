using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.PipelineBehaviors;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.CommandsAndQueries;

public static class ServiceCollectionExtensions
{
    public static void AddCommandsAndQueries(
        this IServiceCollection services,
        params Type[] assemblyMarkerTypes)
    {
        if (assemblyMarkerTypes.Length == 0) throw new ArgumentOutOfRangeException(nameof(assemblyMarkerTypes), "At least one assembly marker type is required");

        services.AddMediatR(assemblyMarkerTypes);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionLogBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));
        services.AddCosmosDbRepository<
            CommandExecutionEvent,
            ICommandExecutionEventRepository,
            CommandExecutionEventCosmosDbRepository>(
            CosmosContainerNames.COMMAND_EXECUTIONS,
            (logger, container) => new CommandExecutionEventCosmosDbRepository(logger, container));
    }
}