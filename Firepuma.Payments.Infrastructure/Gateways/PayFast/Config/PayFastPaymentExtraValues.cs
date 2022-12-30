namespace Firepuma.Payments.Infrastructure.Gateways.PayFast.Config;

public class PayFastPaymentExtraValues
{
    public required string EmailAddress { get; init; }
    public required string NameFirst { get; init; }
    public required double ImmediateAmountInRands { get; init; }
    public required string ItemName { get; init; }
    public required string ItemDescription { get; init; }

    public string? PayfastPaymentToken { get; set; }
}