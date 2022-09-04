﻿using System;
using AutoMapper;
using Firepuma.Payments.Core.Infrastructure.PipelineBehaviors;
using Firepuma.Payments.FunctionAppManager;
using Firepuma.Payments.FunctionAppManager.Gateways.PayFast;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Constants;
using Firepuma.Payments.Infrastructure.CommandHandling;
using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.ServiceMonitoring;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.Payments.FunctionAppManager;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

        var paymentsServiceFunctionsUrl = new Uri(EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsUrl"));
        var paymentsServiceFunctionsKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsKey");

        services.Configure<PaymentsServiceOptions>(config =>
        {
            config.FunctionsUrl = paymentsServiceFunctionsUrl;
            config.FunctionsKey = paymentsServiceFunctionsKey;
        });

        services
            .AddHttpClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = paymentsServiceFunctionsUrl;
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.Add("x-functions-key", paymentsServiceFunctionsKey);
            });

        AddCosmosDb(services);
        AddMediator(services);

        services.AddServiceMonitoring();

        services.AddCommandHandling();

        services.AddPaymentsManagementFeature();
        services.AddPayFastManagerFeature();
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediatR(typeof(Startup));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionLogBehavior<,>));
    }

    private static void AddCosmosDb(IServiceCollection services)
    {
        var connectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosConnectionString");
        var databaseId = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosDatabaseId");
        var client = new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
        var database = client.GetDatabase(databaseId);
        services.AddSingleton(_ => database);
    }
}