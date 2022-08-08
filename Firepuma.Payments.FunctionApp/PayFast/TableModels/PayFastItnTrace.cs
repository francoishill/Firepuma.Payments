using System;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.PayFast.TableModels;

public class PayFastItnTrace : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string PayfastNotificationJson { get; set; }
    public string IncomingRequestUri { get; set; }
    public string PaymentId { get; set; }
    public string PayfastInternalTransactionId { get; set; }

    public PayFastItnTrace(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        string payfastInternalTransactionId,
        string payfastNotificationJson,
        string incomingRequestUri)
    {
        PartitionKey = applicationId.Value;
        RowKey = Guid.NewGuid().ToString();

        PayfastNotificationJson = payfastNotificationJson;
        IncomingRequestUri = incomingRequestUri;
        PaymentId = paymentId.Value;
        PayfastInternalTransactionId = payfastInternalTransactionId;
    }
}