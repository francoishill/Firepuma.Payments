using System.Text.Json;
using Firepuma.Payments.Domain.Payments.ValueObjects;

#pragma warning disable CS8618

namespace Firepuma.Payments.Domain.Payments.Abstractions;

public class PreparePaymentRequest
{
    public PaymentId PaymentId { get; init; }
    public JsonDocument ExtraValues { get; init; }

    public bool TryCastExtraValuesToType<T>(out T extraValues, out string? error) where T : class
    {
        try
        {
            extraValues = ExtraValues.Deserialize<T>()!;

            if (extraValues == null)
            {
                throw new InvalidCastException($"Unable to cast ExtraValues to type {typeof(T).FullName}");
            }

            error = null;
            return true;
        }
        catch (Exception exception)
        {
            extraValues = null!;
            error = exception.Message;
            return false;
        }
    }
}