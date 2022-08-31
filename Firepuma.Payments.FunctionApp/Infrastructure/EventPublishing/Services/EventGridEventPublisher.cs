using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Serialization;
using Azure.Messaging.EventGrid;
using Firepuma.Payments.Core.Infrastructure.Events;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;

public class EventGridEventPublisher : IEventPublisher
{
    private readonly IOptions<EventGridOptions> _eventGridOptions;
    private readonly ILogger<EventGridEventPublisher> _logger;
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    private readonly JsonObjectSerializer _jsonObjectSerializer = new(
        new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });

    public EventGridEventPublisher(
        IOptions<EventGridOptions> eventGridOptions,
        ILogger<EventGridEventPublisher> logger,
        EventGridPublisherClient eventGridPublisherClient)
    {
        _eventGridOptions = eventGridOptions;
        _logger = logger;
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    public async Task PublishAsync<T>(
        T eventData,
        CancellationToken cancellationToken) where T : IPaymentEventGridMessage
    {
        try
        {
            const string eventVersion = "1.0.0";
            var eventType = PaymentEventMappings.GetEventTypeName(eventData);
            var eventGridSubject = _eventGridOptions.Value.SubjectFactory(eventData.ApplicationId);

            var serializedEventData = await _jsonObjectSerializer.SerializeAsync(eventData, cancellationToken: cancellationToken);
            var eventGridEvent = new EventGridEvent(eventGridSubject, eventType, eventVersion, serializedEventData);

            _logger.LogInformation(
                "Sending event with eventId {EventId}, eventType {Type}, subject {Subject}, version {Version}, topic {Topic}",
                eventGridEvent.Id, eventType, eventGridSubject, eventVersion, eventGridEvent.Topic);

            await _eventGridPublisherClient.SendEventAsync(eventGridEvent, cancellationToken);

            _logger.LogInformation(
                "Sent event with eventId {EventId}, eventType {Type}, subject {Subject}, version {Version}, topic {Topic}",
                eventGridEvent.Id, eventType, eventGridSubject, eventVersion, eventGridEvent.Topic);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(
                exception,
                "Unable to publish event, error: {ExceptionMessage}, event data: {SerializeObject}, stack trace: {ExceptionStackTrace}",
                exception.Message, JsonConvert.SerializeObject(eventData, new Newtonsoft.Json.Converters.StringEnumConverter()), exception.StackTrace);
            throw;
        }
    }
}