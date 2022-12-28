using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Config;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Services;

internal class LocalDevelopmentPullPubSubBackgroundService : BackgroundService
{
    private readonly ILogger<LocalDevelopmentPullPubSubBackgroundService> _logger;
    private readonly IOptions<LocalDevelopmentOptions> _options;
    private readonly IBusMessageParser _busMessageParser;
    private readonly IIntegrationEventsMappingCache _mappingCache;
    private readonly IIntegrationEventHandler _eventHandler;

    public LocalDevelopmentPullPubSubBackgroundService(
        ILogger<LocalDevelopmentPullPubSubBackgroundService> logger,
        IOptions<LocalDevelopmentOptions> options,
        IBusMessageParser busMessageParser,
        IIntegrationEventsMappingCache mappingCache,
        IIntegrationEventHandler eventHandler)
    {
        _logger = logger;
        _options = options;
        _busMessageParser = busMessageParser;
        _mappingCache = mappingCache;
        _eventHandler = eventHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(
            _options.Value.PubSubPullProjectId,
            _options.Value.PubSubPullSubscriptionId);

        var subscriber = await SubscriberClient.CreateAsync(subscriptionName);

        var listenTask = WrapListenTask(subscriber.StartAsync(HandleMessageAsync));

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        await subscriber.StopAsync(CancellationToken.None);
        await listenTask;
    }

    private async Task WrapListenTask(Task listenTask)
    {
        try
        {
            await listenTask;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to listen for incoming PubSub messages");
        }
    }

    private async Task<SubscriberClient.Reply> HandleMessageAsync(PubsubMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var messageText = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());
            var messageTextBase64 = ToBase64(messageText);
            var defaultMessageEnvelope = new DefaultPubSubMessageEnvelope
            {
                Subscription = _options.Value.PubSubPullSubscriptionId,
                Message = new DefaultPubSubMessageEnvelope.MessageData
                {
                    Attributes = message.Attributes.ToDictionary(x => x.Key, x => x.Value),
                    MessageId = message.MessageId,
                    Data = messageTextBase64,
                    PublishTime = message.PublishTime.ToDateTime(),
                },
            };
            var messageJson = JsonDocument.Parse(JsonSerializer.Serialize(defaultMessageEnvelope));

            if (!_busMessageParser.TryParseMessage(messageJson, out var parsedMessageEnvelope, out var parseFailureReason))
            {
                _logger.LogError("Failed to parse message, parseFailureReason: {ParseFailureReason}", parseFailureReason);
                return SubscriberClient.Reply.Nack;
            }

            if (!_mappingCache.IsIntegrationEventForFirepumaPayments(parsedMessageEnvelope))
            {
                _logger.LogError(
                    "Unknown message type (not an integration event), message id {MessageId}, message type {MessageType}",
                    message.MessageId, parsedMessageEnvelope.MessageType);

                return SubscriberClient.Reply.Nack;
            }

            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
            };
            var integrationEventEnvelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(
                parsedMessageEnvelope.MessagePayload!,
                deserializeOptions);

            if (integrationEventEnvelope == null)
            {
                _logger.LogError(
                    "Deserialization of message to IntegrationEventEnvelope had NULL result, message id {MessageId}, message type {MessageType}",
                    message.MessageId, parsedMessageEnvelope.MessageType);

                return SubscriberClient.Reply.Nack;
            }

            var handled = await _eventHandler.TryHandleEvent(parsedMessageEnvelope.Source, integrationEventEnvelope, cancellationToken);
            if (!handled)
            {
                _logger.LogError(
                    "Integration event was not handled so returning Nack for message id {MessageId}, event type {EventType}",
                    message.MessageId, integrationEventEnvelope.EventType);
                return SubscriberClient.Reply.Nack;
            }

            return SubscriberClient.Reply.Ack;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to handle PubSub pulled message");
            return SubscriberClient.Reply.Nack;
        }
    }

    private static string ToBase64(string str) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));

    private class DefaultPubSubMessageEnvelope
    {
        [JsonPropertyName("subscription")]
        public string Subscription { get; set; } = null!;

        [JsonPropertyName("message")]
        public MessageData Message { get; set; } = null!;

        public class MessageData
        {
            [JsonPropertyName("attributes")]
            public Dictionary<string, string>? Attributes { get; set; }

            [JsonPropertyName("messageId")]
            public string MessageId { get; set; } = null!;

            [JsonPropertyName("data")]
            public string? Data { get; set; }

            [JsonPropertyName("publishTime")]
            public DateTime PublishTime { get; set; }
        }
    }
}