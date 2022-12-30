using System.ComponentModel.DataAnnotations;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast.Config;

public class PayFastAppConfigExtraValues
{
    public bool IsSandbox { get; init; }

    [Required]
    public string MerchantId { get; init; } = null!;

    [Required]
    public string MerchantKey { get; init; } = null!;

    public string PassPhrase { get; init; } = null!;

    // ReSharper disable once EmptyConstructor
    public PayFastAppConfigExtraValues()
    {
        // used by Azure Table deserialization (like in GetEntityAsync method)
    }

    public static PayFastAppConfigExtraValues CreateFromExtraValues(
        Dictionary<string, string> extraValues)
    {
        //TODO: test that this crashes if the dictionary key does not exist
        return new PayFastAppConfigExtraValues
        {
            IsSandbox = extraValues["IsSandbox"] == "true",
            MerchantId = extraValues["MerchantId"],
            MerchantKey = extraValues["MerchantKey"],
            PassPhrase = extraValues["PassPhrase"],
        };
    }

    public static Dictionary<string, string> CreateExtraValuesDictionary(
        bool isSandbox,
        string merchantId,
        string merchantKey,
        string passPhrase)
    {
        return new Dictionary<string, string>
        {
            ["IsSandbox"] = isSandbox ? "true" : "false",
            ["MerchantId"] = merchantId,
            ["MerchantKey"] = merchantKey,
            ["PassPhrase"] = passPhrase,
        };
    }
}