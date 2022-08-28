using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionAppManager;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsManagementFeature(
        this IServiceCollection services)
    {
        services.AddSingleton<IPaymentApplicationConfigRepository, PaymentApplicationConfigCosmosDbRepository>(s =>
        {
            var database = s.GetRequiredService<Database>();
            var container = database.GetContainer("ApplicationConfigs");
            return new PaymentApplicationConfigCosmosDbRepository(container);
        });
    }
}