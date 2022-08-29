using Firepuma.Payments.Core.Infrastructure.CommandHandling.PipelineBehaviors;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels;
using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandHandling(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));

        services.AddCosmosDbRepository<
            CommandExecutionEvent,
            ICommandExecutionEventRepository,
            CommandExecutionEventCosmosDbRepository>(
            CosmosContainerNames.COMMAND_EXECUTIONS,
            container => new CommandExecutionEventCosmosDbRepository(container));
    }
}