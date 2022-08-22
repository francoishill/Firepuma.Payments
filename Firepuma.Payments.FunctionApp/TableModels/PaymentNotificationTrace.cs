using System;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.TableModels;

public class PaymentNotificationTrace : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string PaymentId { get; set; }
    public string GatewayInternalTransactionId { get; set; }
    public string PaymentNotificationJson { get; set; }
    public string IncomingRequestUri { get; set; }

    public PaymentNotificationTrace(
        ClientApplicationId applicationId,
        string rowKey,
        PaymentId paymentId,
        string gatewayInternalTransactionId,
        string paymentNotificationJson,
        string incomingRequestUri)
    {
        PartitionKey = applicationId.Value;
        RowKey = rowKey;

        PaymentId = paymentId.Value;
        GatewayInternalTransactionId = gatewayInternalTransactionId;
        PaymentNotificationJson = paymentNotificationJson;
        IncomingRequestUri = incomingRequestUri;
    }
}