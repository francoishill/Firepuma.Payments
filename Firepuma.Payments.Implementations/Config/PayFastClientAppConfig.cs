using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Implementations.Config;

public class PayFastClientAppConfig
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    // ReSharper disable once UnusedMember.Global
    public PayFastClientAppConfig()
    {
        // used by Azure Table deserialization (like in GetEntityAsync method)
    }

    public PayFastClientAppConfig(
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