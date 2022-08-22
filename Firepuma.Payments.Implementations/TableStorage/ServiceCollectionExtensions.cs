using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Implementations.TableStorage;

public static class ServiceCollectionExtensions
{
    public static void AddTableProvider<TEntity>(
        this IServiceCollection services,
        string tableName)
        where TEntity : class, ITableEntity
    {
        services.AddSingleton<ITableService<TEntity>>(provider => new TableService<TEntity>(provider.GetRequiredService<TableServiceClient>().GetTableClient(tableName)));

        //TODO: Find a better way
        services.BuildServiceProvider().GetRequiredService<ITableService<TEntity>>().CreateTableIfNotExists();
    }
}