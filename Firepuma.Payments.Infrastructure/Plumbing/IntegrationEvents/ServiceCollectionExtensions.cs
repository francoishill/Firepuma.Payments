using Firepuma.BusMessaging.GooglePubSub;
using Firepuma.EventMediation.IntegrationEvents;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Services;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents.Config;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents;

public static class ServiceCollectionExtensions
{
    public static void AddIntegrationEvents(
        this IServiceCollection services,
        IConfigurationSection integrationEventsConfigSection)
    {
        if (integrationEventsConfigSection == null) throw new ArgumentNullException(nameof(integrationEventsConfigSection));

        services.AddOptions<IntegrationEventsOptions>().Bind(integrationEventsConfigSection).ValidateDataAnnotations().ValidateOnStart();

        services.AddGooglePubSubPublisherClientCache();
        services.AddGooglePubSubMessageParser();

        services.AddTransient<IIntegrationEventsMappingCache, IntegrationEventsMappingCache>();

        services.AddIntegrationEventPublishing<
            IntegrationEventsMappingCache,
            GooglePubSubIntegrationEventPublisher>();

        services.AddIntegrationEventReceiving<
            IntegrationEventsMappingCache,
            IntegrationEventWithCommandsFactoryHandler>();

        services.AddIntegrationEventPublishingForCommandExecution();
    }
}