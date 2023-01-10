using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services.Results;
using Firepuma.Dtos.Notifications.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;

namespace Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Services;

public class IntegrationEventsMappingCache :
    IIntegrationEventsMappingCache,
    IIntegrationEventTypeProvider,
    IIntegrationEventDeserializer
{
    public bool IsIntegrationEventForFirepumaPayments(string messageType)
    {
        return messageType.StartsWith("Firepuma/Payments/Request/", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsIntegrationEventForFirepumaPayments(BusMessageEnvelope envelope)
    {
        return IsIntegrationEventForFirepumaPayments(envelope.MessageType);
    }

    public bool IsIntegrationEventForNotificationsService(string messageType)
    {
        return messageType.StartsWith("Firepuma/Request/Notifications/", StringComparison.OrdinalIgnoreCase);
    }

    public bool TryGetIntegrationEventType<TMessage>(TMessage messagePayload, [NotNullWhen(true)] out string? eventType)
    {
        eventType = messagePayload switch
        {
            ValidatePaymentNotificationCommand.Result => "Firepuma/Payments/Request/PaymentNotificationValidated",

            // events handled by external services
            SendEmailRequest => "Firepuma/Request/Notifications/SendEmail",

            _ => null,
        };

        return eventType != null;
    }

    public bool TryDeserializeIntegrationEvent(
        IntegrationEventEnvelope envelope,
        [NotNullWhen(true)] out object? eventPayload)
    {
        eventPayload = envelope.EventType switch
        {
            "Firepuma/Payments/Request/PaymentNotificationValidated" => DeserializePayload<ValidatePaymentNotificationCommand.Result>(envelope.EventPayload),

            _ => null,
        };

        return eventPayload != null;
    }

    private static TIntegrationEvent? DeserializePayload<TIntegrationEvent>(string eventPayload)
    {
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        return JsonSerializer.Deserialize<TIntegrationEvent?>(eventPayload, deserializeOptions);
    }
}