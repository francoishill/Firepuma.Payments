using System;
using AutoMapper;
using Firepuma.Email.Client;
using Firepuma.Email.Client.Services;
using Firepuma.Payments.FunctionAppManager;
using Firepuma.Payments.FunctionAppManager.Gateways.PayFast;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Constants;
using Firepuma.Payments.Infrastructure.CommandHandling;
using Firepuma.Payments.Infrastructure.Config;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Firepuma.Payments.Infrastructure.ServiceMonitoring;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.Payments.FunctionAppManager;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.GetContext().Configuration;

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

        var cosmosConnectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosConnectionString");
        var cosmosDatabaseId = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosDatabaseId");
        services.AddCosmosDbRepositoriesForFunction(
            cosmosConnectionString,
            cosmosDatabaseId);

        var assembliesWithCommandHandlers = new[]
        {
            typeof(Startup).Assembly,
            typeof(Firepuma.Payments.Infrastructure.ServiceMonitoring.ServiceCollectionExtensions).Assembly,
        };
        services.AddCommandHandlingAndMediatR(assembliesWithCommandHandlers);

        services.AddServiceMonitoring();

        var alertRecipientEmail = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("EmailServiceClient__AlertRecipientEmail");
        services.Configure<AdditionalEmailServiceClientOptions>(config =>
        {
            config.AlertRecipientEmail = alertRecipientEmail;
        });

        services.AddEmailServiceClient(configuration.GetSection("EmailServiceClient"));
        services.BuildServiceProvider().GetRequiredService<IEmailEnqueuingClient>(); //FIX: find a better way (consider validating on startup in Email library code)

        services.AddPaymentsManagementFeature();
        services.AddPayFastManagerFeature();
    }
}