using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.Domain.Payments.Config;

public class PaymentWebhookUrlsOptions
{
    [Required]
    public string IncomingPaymentNotificationWebhookBaseUrl { get; set; } = null!;
}