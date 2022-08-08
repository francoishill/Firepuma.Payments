using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.TableStorage;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Implementations.Config;

public class PayFastClientAppConfig : BaseAzureTable, IPaymentApplicationConfig
{
    public PaymentGatewayTypeId GatewayTypeId => new(PartitionKey);
    public ClientApplicationId ApplicationId => new(RowKey);

    [Required]
    public string ApplicationSecret { get; set; }

    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    // ReSharper disable once UnusedMember.Global
    public PayFastClientAppConfig()
    {
        PartitionKey = "PayFast";
    }

    public PayFastClientAppConfig(
        string applicationId,
        string applicationSecret,
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
        : this()
    {
        RowKey = applicationId;

        ApplicationSecret = applicationSecret;
        IsSandbox = isSandbox;
        MerchantId = merchantId;
        MerchantKey = merchantKey;
        PassPhrase = passPhrase;
    }

    public static string GenerateRandomSecret()
    {
        var key256 = new byte[32];

        using (var rngCryptoServiceProvider = RandomNumberGenerator.Create())
        {
            rngCryptoServiceProvider.GetBytes(key256);
        }

        return Convert.ToBase64String(key256);
    }
}