namespace Firepuma.Payments.FunctionApp.PayFast.ValueObjects;

public class PayFastPaymentSettings
{
    // See PayFastSettings in PayFast nuget package

    public string MerchantId { get; set; }

    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    public string ProcessUrl { get; set; }

    public string ValidateUrl { get; set; }

    public string ReturnUrl { get; set; }

    public string CancelUrl { get; set; }

    public string NotifyUrl { get; set; }
}