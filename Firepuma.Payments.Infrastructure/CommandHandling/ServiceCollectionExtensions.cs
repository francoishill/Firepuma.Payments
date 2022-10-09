using System.Reflection;
using Firepuma.CommandsAndQueries.CosmosDb;
using Firepuma.CommandsAndQueries.CosmosDb.Config;
using Firepuma.Payments.Infrastructure.CommandHandling.Services;
using Firepuma.Payments.Infrastructure.Config;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandsAndQueriesFunctionalityForFunction(
        this IServiceCollection services,
        Assembly[] assembliesWithCommandHandlers)
    {
        services
            .AddCommandHandlingWithCosmosDbStorage(
                new CosmosDbCommandHandlingOptions
                {
                    AddWrapCommandExceptionsPipelineBehavior = true,
                    AddLoggingScopePipelineBehavior = true,
                    AddPerformanceLoggingPipelineBehavior = true,

                    AddValidationBehaviorPipeline = true,
                    ValidationHandlerMarkerAssemblies = assembliesWithCommandHandlers,

                    AddAuthorizationBehaviorPipeline = true,
                    AuthorizationFailurePartitionKeyGenerator = typeof(AuthorizationFailurePartitionKeyGenerator),
                    AuthorizationFailureEventContainerName = CosmosContainerConfiguration.AuthorizationFailures.ContainerProperties.Id,
                    AuthorizationHandlerMarkerAssemblies = assembliesWithCommandHandlers,

                    AddRecordingOfExecution = true,
                    CommandExecutionPartitionKeyGenerator = typeof(CommandExecutionPartitionKeyGenerator),
                    CommandExecutionEventContainerName = CosmosContainerConfiguration.CommandExecutions.ContainerProperties.Id,
                });

        services.AddMediatR(assembliesWithCommandHandlers);
    }
}