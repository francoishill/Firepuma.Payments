using System;
using AutoMapper;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.FunctionApp;
using Firepuma.Payments.FunctionApp.Gateways.PayFast;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Config;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;
using Firepuma.Payments.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.Infrastructure.Config;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable RedundantTypeArgumentsOfMethod

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.Payments.FunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        AddAutoMapper(services);

        var mediationMarkerTypes = new[] { typeof(Startup), typeof(Payments.Infrastructure.CommandsAndQueries.ServiceCollectionExtensions) };
        services.AddCommandsAndQueries(mediationMarkerTypes);

        AddCosmosDb(services);
        AddServiceBusPaymentsMessageSender(services);
        AddEventPublisher(services);

        services.AddPayFastFeature();

        var validateAndStorePaymentNotificationBaseUrl = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("FirepumaValidateAndStorePaymentNotificationBaseUrl");
        services.AddPaymentsFeature(validateAndStorePaymentNotificationBaseUrl);
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }

    private static void AddCosmosDb(IServiceCollection services)
    {
        var connectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosConnectionString");
        var databaseId = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("CosmosDatabaseId");
        var client = new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
        var database = client.GetDatabase(databaseId);
        services.AddSingleton(_ => database);
    }

    private static void AddServiceBusPaymentsMessageSender(IServiceCollection services)
    {
        var connectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("ServiceBus");
        var queueName = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("QueueName");

        services.AddSingleton<ServiceBusClient>(_ =>
            new ServiceBusClient(connectionString));

        services.AddSingleton<ServiceBusSender>(s =>
        {
            var serviceBusClient = s.GetRequiredService<ServiceBusClient>();
            return serviceBusClient.CreateSender(queueName);
        });

        services.AddSingleton<IPaymentsMessageSender, ServiceBusPaymentsMessageSender>();
    }

    private static void AddEventPublisher(IServiceCollection services)
    {
        var eventGridEndpoint = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("EventGridEndpoint");
        var eventGridKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("EventGridAccessKey");

        services.Configure<EventGridOptions>(opt =>
        {
            opt.EventGridEndpoint = eventGridEndpoint;
            opt.EventGridAccessKey = eventGridKey;
            opt.SubjectFactory = applicationId => $"firepuma/payments-service/{applicationId.Value}";
        });

        services.AddSingleton<EventGridPublisherClient>(s =>
        {
            var options = s.GetRequiredService<IOptions<EventGridOptions>>();

            return new EventGridPublisherClient(
                new Uri(options.Value.EventGridEndpoint),
                new AzureKeyCredential(options.Value.EventGridAccessKey));
        });

        services.AddSingleton<IEventPublisher, EventGridEventPublisher>();
    }
}