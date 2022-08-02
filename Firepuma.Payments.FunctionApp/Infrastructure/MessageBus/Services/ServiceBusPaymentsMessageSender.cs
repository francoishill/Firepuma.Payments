using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Mappings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;

public class ServiceBusPaymentsMessageSender : IPaymentsMessageSender
{
    private readonly ILogger<ServiceBusPaymentsMessageSender> _logger;
    private readonly ServiceBusSender _serviceBusSender;

    public ServiceBusPaymentsMessageSender(
        ILogger<ServiceBusPaymentsMessageSender> logger,
        ServiceBusSender serviceBusSender)
    {
        _logger = logger;
        _serviceBusSender = serviceBusSender;
    }

    public async Task SendAsync<T>(
        T messageDto,
        string correlationId,
        CancellationToken cancellationToken) where T : IPaymentBusMessage
    {
        try
        {
            var messageTypeName = PaymentBusMessageMappings.GetMessageTypeName(messageDto);

            var busMessage = new ServiceBusMessage(JsonConvert.SerializeObject(messageDto, new Newtonsoft.Json.Converters.StringEnumConverter()))
            {
                MessageId = Guid.NewGuid().ToString(),
                ApplicationProperties =
                {
                    [PaymentBusMessageMappings.BUS_MESSAGE_TYPE_PROPERTY_KEY] = messageTypeName,
                },
                CorrelationId = correlationId,
            };

            _logger.LogInformation(
                "Sending message with messageId {MessageId}, messageType {Type}, correlationId {CorrelationId}",
                busMessage.MessageId, messageTypeName, correlationId);

            await _serviceBusSender.SendMessageAsync(busMessage, cancellationToken);

            _logger.LogInformation(
                "Sent message with messageId {MessageId}, messageType {Type}, correlationId {CorrelationId}",
                busMessage.MessageId, messageTypeName, correlationId);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(
                exception,
                "Unable to send message, error: {ExceptionMessage}, message data: {SerializeObject}, stack trace: {ExceptionStackTrace}",
                exception.Message, JsonConvert.SerializeObject(messageDto, new Newtonsoft.Json.Converters.StringEnumConverter()), exception.StackTrace);
            throw;
        }
    }
}