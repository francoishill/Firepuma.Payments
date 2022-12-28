using System.Reflection;
using Firepuma.CommandsAndQueries.MongoDb;
using Firepuma.CommandsAndQueries.MongoDb.Config;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.Plumbing.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandsAndQueriesFunctionality(
        this IServiceCollection services,
        string authorizationFailuresCollectionName,
        string commandExecutionsCollectionName,
        Assembly[] assembliesWithCommandHandlers)
    {
        if (authorizationFailuresCollectionName == null) throw new ArgumentNullException(nameof(authorizationFailuresCollectionName));
        if (commandExecutionsCollectionName == null) throw new ArgumentNullException(nameof(commandExecutionsCollectionName));

        assembliesWithCommandHandlers = assembliesWithCommandHandlers.Distinct().ToArray();

        if (assembliesWithCommandHandlers.Length == 0)
        {
            throw new ArgumentException($"At least one assembly is required for {nameof(assembliesWithCommandHandlers)}", nameof(assembliesWithCommandHandlers));
        }

        services
            .AddCommandHandlingWithMongoDbStorage(
                new MongoDbCommandHandlingOptions
                {
                    AddWrapCommandExceptionsPipelineBehavior = true,
                    AddLoggingScopePipelineBehavior = true,
                    AddPerformanceLoggingPipelineBehavior = true,

                    AddValidationBehaviorPipeline = true,
                    ValidationHandlerMarkerAssemblies = assembliesWithCommandHandlers,

                    AddAuthorizationBehaviorPipeline = true,
                    AuthorizationFailureEventCollectionName = authorizationFailuresCollectionName,
                    AuthorizationHandlerMarkerAssemblies = assembliesWithCommandHandlers,

                    AddRecordingOfExecution = true,
                    CommandExecutionEventCollectionName = commandExecutionsCollectionName,
                });

        services.AddMediatR(assembliesWithCommandHandlers);
    }
}