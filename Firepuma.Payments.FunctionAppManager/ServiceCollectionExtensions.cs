using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Firepuma.Payments.Infrastructure.PaymentAppConfiguration.Repositories;
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
            CosmosContainerNames.APPLICATION_CONFIGS,
            container => new PaymentApplicationConfigCosmosDbRepository(container));
    }
}