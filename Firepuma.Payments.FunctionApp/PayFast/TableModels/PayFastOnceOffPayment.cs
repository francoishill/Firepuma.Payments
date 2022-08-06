using System;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.TableModels;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableModels;

public class PayFastOnceOffPayment : TableEntity, IPaymentTableEntity
{
    public string ApplicationId => PartitionKey;

    public PaymentId PaymentId => new(RowKey);

    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }

    public string PayfastPaymentToken { get; set; }

    public string Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }


    // ReSharper disable once UnusedMember.Local
    public PayFastOnceOffPayment()
    {
        // used by TableOperation.Retrieve code
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