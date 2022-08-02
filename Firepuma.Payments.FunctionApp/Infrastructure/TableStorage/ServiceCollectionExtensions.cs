using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;

public static class ServiceCollectionExtensions
{
    public static void AddTableProvider<TProvider>(
        this IServiceCollection services,
        string tableName,
        Func<CloudTable, TProvider> factory)
        where TProvider : class, ITableProvider
    {
        services.AddScoped(s =>
        {
            var storageAccount = s.GetRequiredService<CloudStorageAccount>();
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);

            return factory(table);
        });

        //TODO: Find a better way
        services.BuildServiceProvider().GetRequiredService<TProvider>().Table.CreateIfNotExists();
    }
}