using System;
using Firepuma.Payments.Abstractions.ValueObjects;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableModels;

public class PayFastItnTrace : TableEntity
{
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