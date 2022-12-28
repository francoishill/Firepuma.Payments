using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.BusMessaging.GooglePubSub.Config;
using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.PubSubHandling.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PubSubListenerController : ControllerBase
{
    private readonly ILogger<PubSubListenerController> _logger;
    private readonly IBusMessageParser _busMessageParser;
    private readonly IIntegrationEventsMappingCache _mappingCache;
    private readonly IIntegrationEventHandler _integrationEventHandler;
    private readonly IMongoIndexesApplier _mongoIndexesApplier;

    public PubSubListenerController(
        ILogger<PubSubListenerController> logger,
        IBusMessageParser busMessageParser,
        IIntegrationEventsMappingCache mappingCache,
        IIntegrationEventHandler integrationEventHandler,
        IMongoIndexesApplier mongoIndexesApplier)
    {
        _logger = logger;
        _busMessageParser = busMessageParser;
        _mappingCache = mappingCache;
        _integrationEventHandler = integrationEventHandler;
        _mongoIndexesApplier = mongoIndexesApplier;
    }

    [HttpPost]
    public async Task<IActionResult> HandleBusMessageAsync(
        JsonDocument requestBody,
        CancellationToken cancellationToken)
    {
        if (!_busMessageParser.TryParseMessage(requestBody, out var parsedMessageEnvelope, out var parseFailureReason))
        {
            if (
                requestBody.RootElement.TryGetProperty("message", out var pubSubMessage)
                && pubSubMessage.TryGetProperty("data", out var messageData)
                && DataContainsGithubWorkflowEventName(messageData.GetString() ?? "{}", out var githubWorkflowEventName)
                && string.Equals(githubWorkflowEventName, "NewRevisionDeployed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Detected a GithubWorkflowEventName message for 'NewRevisionDeployed', now running once-off logic after new deployments");

                await _mongoIndexesApplier.ApplyAllIndexes(cancellationToken);

                return Ok("Detected a GithubWorkflowEventName message for 'NewRevisionDeployed', ran once-off logic after new deployments");
            }

            _logger.LogError("Failed to parse message, parseFailureReason: {ParseFailureReason}", parseFailureReason);
            _logger.LogDebug("Message that failed to parse had body: {Body}", JsonSerializer.Serialize(requestBody));
            return BadRequest(parseFailureReason);
        }

        _logger.LogDebug(
            "Parsed message: id {Id}, type: {Type}, payload: {Payload}",
            parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.MessagePayload);

        if (!_mappingCache.IsIntegrationEventForFirepumaPayments(parsedMessageEnvelope))
        {
            _logger.LogError(
                "Unknown message type (not an integration event), message id {MessageId}, message type {MessageType}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType);

            return BadRequest("Unknown message type (not an integration event)");
        }

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        var messagePayload = JsonSerializer.Deserialize<JsonDocument>(parsedMessageEnvelope.MessagePayload ?? "{}", deserializeOptions);

        if (messagePayload == null)
        {
            _logger.LogError(
                "Parsed message deserialization resulted in NULL, message id {MessageId}, type {MessageType}, source: {Source}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.Source);

            return BadRequest("Parsed message deserialization resulted in NULL");
        }

        var integrationEventEnvelope =
            parsedMessageEnvelope.MessageId != BusMessagingPubSubConstants.LOCAL_DEVELOPMENT_PARSED_MESSAGE_ID
                ? messagePayload.Deserialize<IntegrationEventEnvelope>()
                : new IntegrationEventEnvelope // this version is typically used for local development
                {
                    EventId = parsedMessageEnvelope.MessageId,
                    EventType = parsedMessageEnvelope.MessageType,
                    EventPayload = parsedMessageEnvelope.MessagePayload!,
                };

        if (integrationEventEnvelope == null)
        {
            _logger.LogError(
                "IntegrationEventEnvelope deserialization resulted in a NULL, message id {MessageId}, type {MessageType}, source: {Source}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.Source);

            return BadRequest("IntegrationEventEnvelope deserialization resulted in a NULL");
        }

        var handled = await _integrationEventHandler.TryHandleEvent(parsedMessageEnvelope.Source, integrationEventEnvelope, cancellationToken);
        if (!handled)
        {
            _logger.LogError(
                "Integration event was not handled for message id {MessageId}, event type {EventType}",
                parsedMessageEnvelope.MessageId, integrationEventEnvelope.EventType);
            return BadRequest("Integration event was not handled");
        }

        return Accepted(integrationEventEnvelope);
    }

    private static bool DataContainsGithubWorkflowEventName(
        string data,
        [NotNullWhen(true)] out string? githubWorkflowEventName)
    {
        var dataString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));
        if (dataString.Contains("GithubWorkflowEventName"))
        {
            try
            {
                var deserializeOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                };
                var container = JsonSerializer.Deserialize<GithubWorkflowEventContainer>(dataString, deserializeOptions);
                if (!string.IsNullOrWhiteSpace(container?.GithubWorkflowEventName))
                {
                    githubWorkflowEventName = container.GithubWorkflowEventName;
                    return true;
                }
            }
            catch (Exception)
            {
                githubWorkflowEventName = null;
                return false;
            }
        }

        githubWorkflowEventName = null;
        return false;
    }

    private class GithubWorkflowEventContainer
    {
        public string GithubWorkflowEventName { get; set; } = null!;
    }
}