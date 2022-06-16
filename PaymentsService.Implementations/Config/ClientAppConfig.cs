using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.PaymentsService.Implementations.Config;

public class ClientAppConfig : TableEntity
{
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
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
    {
        PartitionKey = paymentProviderName;
        RowKey = applicationId;

        IsSandbox = isSandbox;
        MerchantId = merchantId;
        MerchantKey = merchantKey;
        PassPhrase = passPhrase;
    }
}