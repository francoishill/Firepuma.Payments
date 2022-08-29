using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Infrastructure.Config;

public class PayFastAppConfigExtraValues
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

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