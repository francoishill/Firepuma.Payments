using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.Core.Infrastructure.Events;
using Firepuma.Payments.Core.Infrastructure.Events.EventGridMessages;
using Microsoft.Extensions.Options;
using Sample.PaymentsClientApp.Simple.Config;

namespace Sample.PaymentsClientApp.Simple.Services;

internal class ServiceBusBackgroundProcessor : BackgroundService
{
    private readonly ILogger<ServiceBusBackgroundProcessor> _logger;
    private readonly IOptions<PaymentsMicroserviceOptions> _options;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly PaymentUpdatedMessageHandler _paymentUpdatedMessageHandler;

    public ServiceBusBackgroundProcessor(
        ILogger<ServiceBusBackgroundProcessor> logger,
        IOptions<PaymentsMicroserviceOptions> options,
        ServiceBusClient serviceBusClient,
        PaymentUpdatedMessageHandler paymentUpdatedMessageHandler)
    {
        _logger = logger;
        _options = options;
        _serviceBusClient = serviceBusClient;
        _paymentUpdatedMessageHandler = paymentUpdatedMessageHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var queueName = _options.Value.ServiceBusQueueName;

        _logger.LogInformation("Starting to listen for messages on queue '{QueueName}'", queueName);

        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = _options.Value.MaxConcurrentCalls,
        };

        await using var processor = _serviceBusClient.CreateProcessor(queueName, options);

        try
        {
            processor.ProcessMessageAsync += MessageHandler;

            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            await processor.StopProcessingAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            //TODO: should we restart this processor
            _logger.LogCritical(
                exception,
                "Exception occurred with the connection to service bus with the processor, no messages will be processed anymore. Exception message: {Error}",
                exception.Message);
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }

    private async Task MessageHandler(ProcessMessageEventArgs serviceBusMessage)
    {
        var cancellationToken = serviceBusMessage.CancellationToken;

        if (serviceBusMessage.Message.ApplicationProperties.ContainsKey("aeg-event-type"))
        {
            var eventGridEvents = EventGridEvent.ParseMany(serviceBusMessage.Message.Body);

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (PaymentEventMappings.TryGetPaymentEventData(eventGridEvent, out var paymentEvent))
                {
                    if (paymentEvent is PaymentUpdatedEvent paymentUpdatedEvent)
                    {
                        await _paymentUpdatedMessageHandler.HandlePaymentUpdated(paymentUpdatedEvent, cancellationToken);
                    }
                    else
                    {
                        //TODO: do something with other event type
                        _logger.LogError("Unknown payment event type {EventType} with object type {ObjectType} for event ID {Id}",
                            eventGridEvent.EventType, paymentEvent.GetType().FullName, eventGridEvent.Id);
                    }
                }
                else
                {
                    //TODO: do something with unmapped events
                    _logger.LogError("Unknown event type '{EventType}' for event ID '{Id}'", eventGridEvent.EventType, eventGridEvent.Id);
                }
            }
        }
        else
        {
            //TODO: this is a normal service bus message received (not one that came from an Event Grid event)
            _logger.LogWarning("Not implemented: Handling of non-event-grid service bus message with messageId {MessageId}, correlationId {CorrelationId}, message body: {Body}",
                serviceBusMessage.Message.MessageId, serviceBusMessage.Message.CorrelationId, serviceBusMessage.Message.Body.ToString());
        }
    }

    private async Task ErrorHandler(ProcessErrorEventArgs @event)
    {
        _logger.LogError(
            @event.Exception,
            "Error '{Error}' occurred while trying to process service bus message: entity path '{Path}', source '{Source}', namespace '{Namespace}'",
            @event.Exception?.Message, @event.EntityPath, @event.ErrorSource.ToString(), @event.FullyQualifiedNamespace);

        await Task.CompletedTask;
    }
}