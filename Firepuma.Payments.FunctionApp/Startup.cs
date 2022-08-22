using System;
using AutoMapper;
using Azure;
using Azure.Data.Tables;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.FunctionApp;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Config;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;
using Firepuma.Payments.FunctionApp.Infrastructure.PipelineBehaviors;
using Firepuma.Payments.FunctionApp.PayFast;
using MediatR;
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
        AddMediator(services);
        AddCloudStorageAccount(services);
        AddServiceBusPaymentsMessageSender(services);
        AddEventPublisher(services);

        services.AddCommandHandling();

        services.AddPayFastFeature();

        var validateAndStorePaymentNotificationBaseUrl = GetRequiredEnvironmentVariable("FirepumaValidateAndStorePaymentNotificationBaseUrl");
        services.AddPaymentsFeature(validateAndStorePaymentNotificationBaseUrl);
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediatR(typeof(Startup));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionLogBehavior<,>));
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }

    private static void AddCloudStorageAccount(IServiceCollection services)
    {
        var storageConnectionString = GetRequiredEnvironmentVariable("AzureWebJobsStorage");
        services.AddSingleton(_ => new TableServiceClient(storageConnectionString));
    }

    private static void AddServiceBusPaymentsMessageSender(IServiceCollection services)
    {
        var connectionString = GetRequiredEnvironmentVariable("ServiceBus");
        var queueName = GetRequiredEnvironmentVariable("QueueName");

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
        var eventGridEndpoint = GetRequiredEnvironmentVariable("EventGridEndpoint");
        var eventGridKey = GetRequiredEnvironmentVariable("EventGridAccessKey");

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

    private static string GetRequiredEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Environment variable '{key}' is empty but required");
        }

        return value;
    }
}