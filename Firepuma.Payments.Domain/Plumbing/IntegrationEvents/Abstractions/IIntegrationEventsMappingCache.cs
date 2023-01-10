using Firepuma.BusMessaging.Abstractions.Services.Results;

namespace Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;

public interface IIntegrationEventsMappingCache
{
    bool IsIntegrationEventForFirepumaPayments(string messageType);
    bool IsIntegrationEventForFirepumaPayments(BusMessageEnvelope envelope);

    bool IsIntegrationEventForNotificationsService(string messageType);
}