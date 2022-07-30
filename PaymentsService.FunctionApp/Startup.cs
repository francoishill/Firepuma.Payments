using System;
using AutoMapper;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Firepuma.PaymentsService.FunctionApp;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.EventPublishing;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.EventPublishing.Config;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.PipelineBehaviors;
using Firepuma.PaymentsService.FunctionApp.PayFast;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable RedundantTypeArgumentsOfMethod

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.PaymentsService.FunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        AddAutoMapper(services);
        AddMediator(services);
        AddCloudStorageAccount(services);
        AddServiceBus(services);
        AddEventPublisher(services);

        services.AddCommandHandling();

        var validateAndStoreItnBaseUrl = GetRequiredEnvironmentVariable("FirepumaValidateAndStorePayFastItnBaseUrl");
        services.AddPayFastFeature(validateAndStoreItnBaseUrl);
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
        services.AddSingleton<CloudStorageAccount>(CloudStorageAccount.Parse(storageConnectionString));
    }

    private static void AddServiceBus(IServiceCollection services)
    {
        var connectionString = GetRequiredEnvironmentVariable("FirepumaPaymentsServiceBus");
        var queueName = GetRequiredEnvironmentVariable("FirepumaPaymentsQueueName");

        services.AddSingleton<ServiceBusClient>(_ =>
            new ServiceBusClient(connectionString));

        services.AddSingleton<ServiceBusSender>(s =>
        {
            var serviceBusClient = s.GetRequiredService<ServiceBusClient>();
            return serviceBusClient.CreateSender(queueName);
        });
    }

    private static void AddEventPublisher(IServiceCollection services)
    {
        var eventGridEndpoint = GetRequiredEnvironmentVariable("EventGridEndpoint");
        var eventGridKey = GetRequiredEnvironmentVariable("EventGridAccessKey");

        services.Configure<EventGridOptions>(opt =>
        {
            opt.EventGridEndpoint = eventGridEndpoint;
            opt.EventGridAccessKey = eventGridKey;
            opt.SubjectFactory = applicationId => $"firepuma/payments-service/{applicationId}";
        });

        services.AddSingleton<EventGridPublisherClient>(s =>
        {
            var options = s.GetRequiredService<IOptions<EventGridOptions>>();

            return new EventGridPublisherClient(
                new Uri(options.Value.EventGridEndpoint),
                new AzureKeyCredential(options.Value.EventGridAccessKey));
        });

        services.AddSingleton<EventPublisher>();
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