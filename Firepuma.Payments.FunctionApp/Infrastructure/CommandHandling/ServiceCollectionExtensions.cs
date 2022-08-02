using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.PipelineBehaviors;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableProviders;
using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandHandling(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));

        services.AddTableProvider("PaymentsServiceCommandExecutions", table => new CommandExecutionTableProvider(table));
    }
}