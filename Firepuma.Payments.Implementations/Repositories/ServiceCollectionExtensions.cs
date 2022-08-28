using Firepuma.Payments.Abstractions.Entities;
using Firepuma.Payments.Abstractions.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Implementations.Repositories;

public static class ServiceCollectionExtensions
{
    public static void AddCosmosDbRepository<TEntity, TInterface, TClass>(
        this IServiceCollection services,
        string containerName,
        Func<Container, TClass> classFactory)
        where TEntity : BaseEntity, new()
        where TInterface : class, IRepository<TEntity>
        where TClass : class, TInterface
    {
        services.AddSingleton<TInterface, TClass>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer(containerName);
            return classFactory(container);
        });
    }
}