using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services.Results;
using Firepuma.Dtos.Email.BusMessages;
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
        return messageType.StartsWith("Firepuma/FirepumaPayments/", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsIntegrationEventForFirepumaPayments(BusMessageEnvelope envelope)
    {
        return IsIntegrationEventForFirepumaPayments(envelope.MessageType);
    }

    public bool IsIntegrationEventForEmailService(string messageType)
    {
        return messageType.StartsWith("Firepuma/EmailService/", StringComparison.OrdinalIgnoreCase);
    }

    public bool TryGetIntegrationEventType<TMessage>(TMessage messagePayload, [NotNullWhen(true)] out string? eventType)
    {
        eventType = messagePayload switch
        {
            ValidatePaymentNotificationCommand.Result => "Firepuma/FirepumaPayments/Event/PaymentNotificationValidated",

            // events handled by external services
            SendEmailRequest => "Firepuma/EmailService/SendEmail",

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
            "Firepuma/FirepumaPayments/Event/PaymentNotificationValidated" => DeserializePayload<ValidatePaymentNotificationCommand.Result>(envelope.EventPayload),

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