namespace Firepuma.Payments.Infrastructure.Gateways.PayFast.ValueObjects;

public class PayFastPaymentSettings
{
    // See PayFastSettings in PayFast nuget package

    public string MerchantId { get; set; } = null!;

    public string MerchantKey { get; set; } = null!;

    public string PassPhrase { get; set; } = null!;

    public string ProcessUrl { get; set; } = null!;

    public string ValidateUrl { get; set; } = null!;

    public string ReturnUrl { get; set; } = null!;

    public string CancelUrl { get; set; } = null!;

    public string NotifyUrl { get; set; } = null!;
}