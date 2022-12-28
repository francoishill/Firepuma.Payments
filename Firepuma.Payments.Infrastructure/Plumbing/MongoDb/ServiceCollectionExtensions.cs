using Firepuma.DatabaseRepositories.MongoDb;
using Firepuma.Payments.Infrastructure.Plumbing.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.Plumbing.MongoDb;

public static class ServiceCollectionExtensions
{
    public static void AddMongoDbRepositories(
        this IServiceCollection services,
        IConfigurationSection mongoDbConfigSection,
        bool isDevelopmentEnvironment,
        out MongoDbOptions mongoDbOptions)
    {
        if (mongoDbConfigSection == null) throw new ArgumentNullException(nameof(mongoDbConfigSection));

        services.AddOptions<MongoDbOptions>().Bind(mongoDbConfigSection).ValidateDataAnnotations().ValidateOnStart();
        mongoDbOptions = mongoDbConfigSection.Get<MongoDbOptions>()!;

        var tmpMongoDbOptions = mongoDbOptions;
        services.AddMongoDbRepositories(options =>
            {
                options.ConnectionString = tmpMongoDbOptions.ConnectionString;
                options.DatabaseName = tmpMongoDbOptions.DatabaseName;
            },
            validateOnStart: true,
            configureClusterBuilder: _ =>
            {
                if (isDevelopmentEnvironment)
                {
                    // disable this subscription for now

                    // cb.Subscribe<CommandStartedEvent>(e =>
                    // {
                    //     Console.WriteLine($"MongoCommand: {e.CommandName} - {e.Command.ToJson()}");
                    // });
                }
            });
    }
}