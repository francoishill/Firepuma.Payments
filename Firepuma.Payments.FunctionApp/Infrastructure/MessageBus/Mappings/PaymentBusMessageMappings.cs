﻿using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.MessageBus.Mappings;

public static class PaymentBusMessageMappings
{
    public const string BUS_MESSAGE_TYPE_PROPERTY_KEY = "Firepuma.PaymentsService.BusMessageType";

    private static class MessageTypeNames
    {
        public const string PayFastPaymentItnValidatedMessage = "Firepuma.Payments.FunctionApp.PayFastPaymentItnValidatedMessage";
    }

    private static readonly IReadOnlyDictionary<Type, string> _messageTypeNameMap = new Dictionary<Type, string>
    {
        [typeof(PayFastPaymentItnValidatedMessage)] = MessageTypeNames.PayFastPaymentItnValidatedMessage,
    };

    private static readonly IReadOnlyDictionary<string, Func<BinaryData, object>> _messageDeserializers = new Dictionary<string, Func<BinaryData, object>>
    {
        [MessageTypeNames.PayFastPaymentItnValidatedMessage] = messageData => JsonConvert.DeserializeObject<PayFastPaymentItnValidatedMessage>(messageData.ToString()),
    };

    public static string GetMessageTypeName<T>(T eventData) where T : IPaymentBusMessage
    {
        return _messageTypeNameMap[eventData.GetType()];
    }

    public static bool TryGetPaymentMessage(ServiceBusReceivedMessage busMessage, out object eventData)
    {
        try
        {
            if (!busMessage.ApplicationProperties.TryGetValue(BUS_MESSAGE_TYPE_PROPERTY_KEY, out var messageType))
            {
                eventData = null;
                return false;
            }

            var messageTypeString = messageType?.ToString();
            if (string.IsNullOrWhiteSpace(messageTypeString))
            {
                eventData = null;
                return false;
            }

            if (!_messageDeserializers.TryGetValue(messageTypeString, out var deserializationFunction))
            {
                eventData = null;
                return false;
            }

            eventData = deserializationFunction(busMessage.Body);
            return eventData != null;
        }
        catch
        {
            eventData = null;
            return false;
        }
    }
}