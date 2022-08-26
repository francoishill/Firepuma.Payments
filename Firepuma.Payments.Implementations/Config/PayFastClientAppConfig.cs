using System.ComponentModel.DataAnnotations;
using Firepuma.Payments.Abstractions.ValueObjects;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Implementations.Config;

public class PayFastClientAppConfig : BasePaymentApplicationConfig
{
    private static readonly PaymentGatewayTypeId _staticGatewayTypeId = new("PayFast");

    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    // ReSharper disable once UnusedMember.Global
    public PayFastClientAppConfig()
        : base(_staticGatewayTypeId)
    {
        // used by Azure Table deserialization (like in GetEntityAsync method)
    }

    public PayFastClientAppConfig(
        ClientApplicationId applicationId,
        string applicationSecret,
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
        : base(_staticGatewayTypeId, applicationId, applicationSecret)
    {
        IsSandbox = isSandbox;
        MerchantId = merchantId;
        MerchantKey = merchantKey;
        PassPhrase = passPhrase;
    }
}