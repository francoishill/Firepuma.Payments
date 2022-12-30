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

    public static PayFastAppConfigExtraValues CreateFromExtraValues(
        Dictionary<string, string> extraValues)
    {
        return new PayFastAppConfigExtraValues
        {
            IsSandbox = extraValues["IsSandbox"] == "true",
            MerchantId = extraValues["MerchantId"],
            MerchantKey = extraValues["MerchantKey"],
            PassPhrase = extraValues["PassPhrase"],
        };
    }

    public class CreateRequestDto
    {
        public bool IsSandbox { get; set; }

        [Required]
        public string MerchantId { get; set; } = null!;

        [Required]
        public string MerchantKey { get; set; } = null!;

        public string PassPhrase { get; set; } = null!;

        public Dictionary<string, string> ToExtraValuesDictionary()
        {
            return new Dictionary<string, string>
            {
                ["IsSandbox"] = IsSandbox ? "true" : "false",
                ["MerchantId"] = MerchantId,
                ["MerchantKey"] = MerchantKey,
                ["PassPhrase"] = PassPhrase,
            };
        }
    }
}