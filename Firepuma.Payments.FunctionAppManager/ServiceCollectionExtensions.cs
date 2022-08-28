using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.Repositories;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.FunctionAppManager;

public static class ServiceCollectionExtensions
{
    public static void AddPaymentsManagementFeature(
        this IServiceCollection services)
    {
        services.AddCosmosDbRepository<
            PaymentApplicationConfig,
            IPaymentApplicationConfigRepository,
            PaymentApplicationConfigCosmosDbRepository>(
            "ApplicationConfigs",
            container => new PaymentApplicationConfigCosmosDbRepository(container));
    }
}