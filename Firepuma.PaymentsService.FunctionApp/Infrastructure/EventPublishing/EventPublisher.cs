using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Serialization;
using Azure.Messaging.EventGrid;
using Firepuma.PaymentsService.Abstractions.Events;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.EventPublishing.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.EventPublishing;

public class EventPublisher
{
    private readonly IOptions<EventGridOptions> _eventGridOptions;
    private readonly ILogger<EventPublisher> _logger;
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    private readonly JsonObjectSerializer _jsonObjectSerializer = new(
        new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });

    public EventPublisher(
        IOptions<EventGridOptions> eventGridOptions,
        ILogger<EventPublisher> logger,
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

            await _eventGridPublisherClient.SendEventAsync(eventGridEvent, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(
                exception,
                "Unable to publish event grid message, error: {ExceptionMessage}, event data: {SerializeObject}, stack trace: {ExceptionStackTrace}",
                exception.Message, JsonConvert.SerializeObject(eventData, new Newtonsoft.Json.Converters.StringEnumConverter()), exception.StackTrace);
            throw;
        }
    }
}