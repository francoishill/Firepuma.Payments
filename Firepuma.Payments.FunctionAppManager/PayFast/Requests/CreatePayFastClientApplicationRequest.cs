using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.FunctionAppManager.PayFast.Requests;

public class CreatePayFastClientApplicationRequest
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }
}