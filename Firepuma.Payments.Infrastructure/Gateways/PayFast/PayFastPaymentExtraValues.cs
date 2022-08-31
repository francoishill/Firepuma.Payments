namespace Firepuma.Payments.Infrastructure.Gateways.PayFast;

public class PayFastPaymentExtraValues
{
    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }

    public string PayfastPaymentToken { get; set; }

    public PayFastPaymentExtraValues(
        string emailAddress,
        string nameFirst,
        double immediateAmountInRands,
        string itemName,
        string itemDescription)
    {
        EmailAddress = emailAddress;
        NameFirst = nameFirst;
        ImmediateAmountInRands = immediateAmountInRands;
        ItemName = itemName;
        ItemDescription = itemDescription;
    }
}