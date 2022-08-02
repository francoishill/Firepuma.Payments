using System;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableModels;

public class PayFastItnTrace : TableEntity
{
    public string PayfastNotificationJson { get; set; }
    public string IncomingRequestUri { get; set; }
    public string PaymentId { get; set; }
    public string PayfastInternalTransactionId { get; set; }

    public PayFastItnTrace(
        string applicationId,
        string paymentId,
        string payfastInternalTransactionId,
        string payfastNotificationJson,
        string incomingRequestUri)
    {
        PartitionKey = applicationId;
        RowKey = Guid.NewGuid().ToString();

        PayfastNotificationJson = payfastNotificationJson;
        IncomingRequestUri = incomingRequestUri;
        PaymentId = paymentId;
        PayfastInternalTransactionId = payfastInternalTransactionId;
    }
}