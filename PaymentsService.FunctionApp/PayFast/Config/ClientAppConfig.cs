using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Config;

public class ClientAppConfig : TableEntity
{
    public bool IsSandbox { get; set; }

    [Required]
    public string MerchantId { get; set; }

    [Required]
    public string MerchantKey { get; set; }

    public string PassPhrase { get; set; }

    [Required]
    public string WebUiBaseUrl { get; set; }
}