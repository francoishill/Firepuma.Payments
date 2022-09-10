using System.Reflection;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.PipelineBehaviors;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CommandsAndQueries.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using FluentValidation;
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

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(WrapExceptionBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        foreach (var assemblyMarkerType in assemblyMarkerTypes)
        {
            services.AddValidatorsFromAssembly(assemblyMarkerType.Assembly);
        }

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));
        services.AddCosmosDbRepository<
            CommandExecutionEvent,
            ICommandExecutionEventRepository,
            CommandExecutionEventCosmosDbRepository>(
            CosmosContainerNames.COMMAND_EXECUTIONS,
            (logger, container) => new CommandExecutionEventCosmosDbRepository(logger, container));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddCosmosDbRepository<
            AuthorizationFailureEvent,
            IAuthorizationFailureEventRepository,
            AuthorizationFailureEventCosmosDbRepository>(
            CosmosContainerNames.AUTHORIZATION_FAILURES,
            (logger, container) => new AuthorizationFailureEventCosmosDbRepository(logger, container));
        foreach (var assemblyMarkerType in assemblyMarkerTypes)
        {
            services.AddAuthorizersFromAssembly(assemblyMarkerType.Assembly);
        }
    }

    private static void AddAuthorizersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var authorizerType = typeof(IAuthorizer<>);
        assembly.GetTypesAssignableTo(authorizerType).ForEach((type) =>
        {
            foreach (var implementedInterface in type.ImplementedInterfaces)
            {
                services.AddScoped(implementedInterface, type);
            }
        });
    }

    private static List<TypeInfo> GetTypesAssignableTo(this Assembly assembly, Type compareType)
    {
        var typeInfoList = assembly.DefinedTypes.Where(x => x.IsClass
                                                            && !x.IsAbstract
                                                            && x != compareType
                                                            && x.GetInterfaces()
                                                                .Any(i => i.IsGenericType
                                                                          && i.GetGenericTypeDefinition() == compareType)).ToList();

        return typeInfoList;
    }
}