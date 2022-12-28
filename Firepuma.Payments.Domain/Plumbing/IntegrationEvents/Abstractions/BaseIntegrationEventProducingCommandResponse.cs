using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.Factories;

namespace Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;

public abstract class BaseIntegrationEventProducingCommandResponse : IIntegrationEventProducingCommandResponse
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string IntegrationEventId { get; set; } = IntegrationEventIdFactory.GenerateIntegrationEventId();
}