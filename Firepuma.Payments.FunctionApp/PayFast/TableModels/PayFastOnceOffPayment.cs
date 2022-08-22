using System;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.TableModels;

namespace Firepuma.Payments.FunctionApp.PayFast.TableModels;

public class PayFastOnceOffPayment : IPaymentTableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ApplicationId => new(PartitionKey);
    public string PaymentId => RowKey;

    public string GatewayTypeId { get; set; }

    public string Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }

    public string PayfastPaymentToken { get; set; }


    // ReSharper disable once UnusedMember.Local
    public PayFastOnceOffPayment()
    {
        // used by table.GetEntityAsync code
    }

    public PayFastOnceOffPayment(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        string emailAddress,
        string nameFirst,
        double immediateAmountInRands,
        string itemName,
        string itemDescription)
    {
        PartitionKey = applicationId.Value;
        RowKey = paymentId.Value;

        EmailAddress = emailAddress;
        NameFirst = nameFirst;
        ImmediateAmountInRands = immediateAmountInRands;
        ItemName = itemName;
        ItemDescription = itemDescription;
        Status = PayFastSubscriptionStatus.New.ToString();
    }

    public void SetStatus(PayFastSubscriptionStatus status)
    {
        Status = status.ToString();
        StatusChangedOn = DateTime.UtcNow;
    }
}