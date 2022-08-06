using System.ComponentModel.DataAnnotations;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Abstractions.DTOs.Requests;

public class PreparePayFastOnceOffPaymentRequest
{
    [Required]
    public PaymentId PaymentId { get; set; }

    [Required]
    public string BuyerEmailAddress { get; set; }

    [Required]
    public string BuyerFirstName { get; set; }

    [Required]
    public double? ImmediateAmountInRands { get; set; }

    [Required]
    public string ItemName { get; set; }

    [Required]
    public string ItemDescription { get; set; }

    [Required]
    public string ReturnUrl { get; set; }

    [Required]
    public string CancelUrl { get; set; }

    public SplitPaymentConfig SplitPayment { get; set; }

    public class SplitPaymentConfig : IValidatableObject
    {
        public int MerchantId { get; set; }
        public int AmountInCents { get; set; }
        public int Percentage { get; set; }
        public int MinCents { get; set; }
        public int MaxCents { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance as SplitPaymentConfig;
            if (model == null)
            {
                yield break;
            }

            if (model.MerchantId <= 0)
            {
                yield return new ValidationResult(
                    $"{nameof(MerchantId)} is required and must be more than 0",
                    new[] { nameof(MerchantId) });
            }

            if (model.AmountInCents == 0 && model.Percentage == 0)
            {
                yield return new ValidationResult(
                    $"Either {nameof(AmountInCents)} or {nameof(Percentage)} should be more than 0",
                    new[] { nameof(AmountInCents), nameof(Percentage) });
            }
        }
    }
}