using System.Text.Json;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.BusMessaging.Abstractions.Services.Requests;
using Firepuma.BusMessaging.GooglePubSub.Services;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents.Config;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents.Services;

public class GooglePubSubIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<GooglePubSubIntegrationEventPublisher> _logger;
    private readonly IPublisherClientCache _publisherClientCache;
    private readonly IIntegrationEventsMappingCache _mappingCache;
    private readonly TopicName _workerFirepumaPaymentsTopicName;
    private readonly TopicName _notificationsServiceTopicName;

    public GooglePubSubIntegrationEventPublisher(
        ILogger<GooglePubSubIntegrationEventPublisher> logger,
        IOptions<IntegrationEventsOptions> options,
        IPublisherClientCache publisherClientCache,
        IIntegrationEventsMappingCache mappingCache)
    {
        _logger = logger;
        _publisherClientCache = publisherClientCache;
        _mappingCache = mappingCache;

        _workerFirepumaPaymentsTopicName = TopicName.FromProjectTopic(options.Value.FirepumaPaymentsWorkerProjectId, options.Value.FirepumaPaymentsWorkerTopicId);
        _notificationsServiceTopicName = TopicName.FromProjectTopic(options.Value.NotificationsServiceProjectId, options.Value.NotificationsServiceTopicId);
    }

    public async Task SendAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        var messageType = eventEnvelope.EventType;

        var request = new PopulateMessageAttributesRequest
        {
            Source = IntegrationEventConstants.MESSAGE_PUBLISHER_SOURCE_ID,
            MessageType = messageType,
            ContentType = "application/json",
        };

        var eventEnvelopeJson = JsonSerializer.Serialize(eventEnvelope);
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(eventEnvelopeJson),
        };

        var attributes = new Dictionary<string, string>();
        attributes.PopulateMessageAttributes(request);

        message.Attributes.Add(attributes);

        var topic = GetTopicForMessageType(messageType);
        var cacheKey = $"{topic.ProjectId}/{topic.TopicId}";
        var publisher = await _publisherClientCache.GetPublisherClient(topic, cacheKey, cancellationToken);

        _logger.LogDebug(
            "Obtained publisher for message {MessageType}, project: {Project}, topic: {Topic}",
            messageType, publisher.TopicName.ProjectId, publisher.TopicName.TopicId);

        var sentMessageId = await publisher.PublishAsync(message);

        _logger.LogInformation(
            "Message {Id} was successfully published at {Time}, project: {Project}, topic: {Topic}",
            sentMessageId, DateTime.UtcNow.ToString("O"), publisher.TopicName.ProjectId, publisher.TopicName.TopicId);
    }

    private TopicName GetTopicForMessageType(string messageType)
    {
        if (_mappingCache.IsIntegrationEventForFirepumaPayments(messageType))
        {
            return _workerFirepumaPaymentsTopicName;
        }

        if (_mappingCache.IsIntegrationEventForNotificationsService(messageType))
        {
            return _notificationsServiceTopicName;
        }

        _logger.LogError("Message type '{MessageType}' does not have a configured pubsub topic", messageType);
        throw new Exception($"Message type '{messageType}' does not have a configured pubsub topic");
    }
}