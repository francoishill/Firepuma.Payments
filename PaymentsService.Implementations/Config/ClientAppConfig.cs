using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.PaymentsService.Implementations.Config;

public class ClientAppConfig : TableEntity
{
    public string PaymentProviderName => PartitionKey;
    public string ApplicationId => RowKey;

    [Required]
    public string ApplicationSecret { get; set; }

    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    // ReSharper disable once UnusedMember.Global
    public ClientAppConfig()
    {
        // used by input bindings of functions
    }

    public ClientAppConfig(
        string paymentProviderName,
        string applicationId,
        string applicationSecret,
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
    {
        PartitionKey = paymentProviderName;
        RowKey = applicationId;

        ApplicationSecret = applicationSecret;
        IsSandbox = isSandbox;
        MerchantId = merchantId;
        MerchantKey = merchantKey;
        PassPhrase = passPhrase;
    }

    public static TableOperation GetRetrieveOperation(string paymentProviderName, string applicationId)
    {
        return TableOperation.Retrieve<ClientAppConfig>(paymentProviderName, applicationId);
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