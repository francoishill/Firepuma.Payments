using System;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.TableModels;

public class PaymentTrace : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string PaymentId { get; set; }
    public string GatewayInternalTransactionId { get; set; }
    public string PaymentNotificationJson { get; set; }
    public string IncomingRequestUri { get; set; }

    public PaymentTrace(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        string gatewayInternalTransactionId,
        string paymentNotificationJson,
        string incomingRequestUri)
    {
        PartitionKey = applicationId.Value;
        RowKey = Guid.NewGuid().ToString();

        PaymentId = paymentId.Value;
        GatewayInternalTransactionId = gatewayInternalTransactionId;
        PaymentNotificationJson = paymentNotificationJson;
        IncomingRequestUri = incomingRequestUri;
    }
}