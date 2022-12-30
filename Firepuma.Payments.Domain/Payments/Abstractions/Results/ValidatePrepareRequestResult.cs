using Firepuma.Payments.Domain.Payments.Abstractions.ExtraValues;

namespace Firepuma.Payments.Domain.Payments.Abstractions.Results;

public class ValidatePrepareRequestResult
{
    public IPreparePaymentExtraValues ExtraValues { get; init; } = null!;
}