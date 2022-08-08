using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Implementations.TableStorage;

public static class ServiceCollectionExtensions
{
    public static void AddTableProvider<TProvider>(
        this IServiceCollection services,
        string tableName,
        Func<TableClient, TProvider> factory)
        where TProvider : class, ITableProvider
    {
        services.AddScoped(s =>
        {
            var tableServiceClient = s.GetRequiredService<TableServiceClient>();
            var tableClient = tableServiceClient.GetTableClient(tableName);
            return factory(tableClient);
        });

        //TODO: Find a better way
        services.BuildServiceProvider().GetRequiredService<TProvider>().Table.CreateIfNotExists();
    }
}