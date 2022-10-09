using Firepuma.DatabaseRepositories.CosmosDb;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.CosmosDb;

public static class ServiceCollectionExtensions
{
    public static void AddCosmosDbRepositoriesForFunction(
        this IServiceCollection services,
        string connectionString,
        string databaseId)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        if (string.IsNullOrWhiteSpace(databaseId)) throw new ArgumentNullException(nameof(databaseId));

        services.AddCosmosDbRepositories(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseId = databaseId;
            },
            validateOnStart: false);
    }
}