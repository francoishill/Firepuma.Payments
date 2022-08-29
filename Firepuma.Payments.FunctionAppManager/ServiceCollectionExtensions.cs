﻿using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.Repositories;
using Firepuma.Payments.Infrastructure.Repositories.EntityRepositories;
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