using System.Text.Json;
using Firepuma.Payments.Domain.Payments.ValueObjects;

#pragma warning disable CS8618

namespace Firepuma.Payments.Domain.Payments.Abstractions;

public class PreparePaymentRequest
{
    public PaymentId PaymentId { get; set; }
    public JsonDocument ExtraValues { get; set; }

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

    public static JsonDocument CastToExtraValues<T>(T extraValues) where T : class
    {
        //TODO: test this, it is new code
        return JsonSerializer.SerializeToDocument(extraValues);
    }
}