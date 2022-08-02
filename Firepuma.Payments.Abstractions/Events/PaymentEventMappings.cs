using Azure.Messaging.EventGrid;
using Firepuma.PaymentsService.Abstractions.Events.EventGridMessages;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.Abstractions.Events;

public static class PaymentEventMappings
{
    private static readonly IReadOnlyDictionary<Type, string> _eventTypeNameMap = new Dictionary<Type, string>
    {
        [typeof(PayFastPaymentUpdatedEvent)] = "Firepuma.PaymentsService.PayFastPaymentUpdated",
    };

    private static readonly IReadOnlyDictionary<string, Func<BinaryData, object>> _eventDeserializers = new Dictionary<string, Func<BinaryData, object>>
    {
        ["Firepuma.PaymentsService.PayFastPaymentUpdated"] = eventBinaryData => JsonConvert.DeserializeObject<PayFastPaymentUpdatedEvent>(eventBinaryData.ToString()),
    };

    public static string GetEventTypeName<T>(T eventData) where T : IPaymentEventGridMessage
    {
        return _eventTypeNameMap[eventData.GetType()];
    }

    public static bool TryGetPaymentEventData(EventGridEvent eventGridEvent, out object eventData)
    {
        try
        {
            if (!_eventDeserializers.TryGetValue(eventGridEvent.EventType, out var deserializationFunction))
            {
                eventData = null;
                return false;
            }

            eventData = deserializationFunction(eventGridEvent.Data);
            return eventData != null;
        }
        catch
        {
            eventData = null;
            return false;
        }
    }
}