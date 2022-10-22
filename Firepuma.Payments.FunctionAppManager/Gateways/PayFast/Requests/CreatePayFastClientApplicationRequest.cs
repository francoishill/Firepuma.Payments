using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.FunctionAppManager.Gateways.PayFast.Requests;

public class CreatePayFastClientApplicationRequest
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; } = null!;

    [Required]
    public string MerchantKey { get; set; } = null!;

    public string PassPhrase { get; set; } = null!;
}