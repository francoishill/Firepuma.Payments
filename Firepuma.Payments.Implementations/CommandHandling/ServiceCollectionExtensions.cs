using Firepuma.Payments.Implementations.CommandHandling.PipelineBehaviors;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Implementations.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandHandling(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));

        services.AddSingleton<ICommandExecutionEventRepository, CommandExecutionEventCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("CommandExecutions");
            return new CommandExecutionEventCosmosDbRepository(container);
        });
    }
}