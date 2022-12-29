using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast.Config;

public class PayFastAppConfigExtraValues
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; } = null!;

    [Required]
    public string MerchantKey { get; set; } = null!;

    public string PassPhrase { get; set; } = null!;

    // ReSharper disable once UnusedMember.Global
    public PayFastAppConfigExtraValues()
    {
        // used by Azure Table deserialization (like in GetEntityAsync method)
    }

    public PayFastAppConfigExtraValues(
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
    {
        IsSandbox = isSandbox;
        MerchantId = merchantId;
        MerchantKey = merchantKey;
        PassPhrase = passPhrase;
    }
}