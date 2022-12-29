using Firepuma.Payments.Domain.Payments.ValueObjects;
using Newtonsoft.Json.Linq;

#pragma warning disable CS8618

namespace Firepuma.Payments.Domain.Payments.Abstractions;

public class PreparePaymentRequest
{
    public PaymentId PaymentId { get; set; }
    public JObject ExtraValues { get; set; }

    public bool TryCastExtraValuesToType<T>(out T extraValues, out string? error) where T : class
    {
        try
        {
            extraValues = ExtraValues.ToObject<T>()!;

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

    public static JObject CastToExtraValues<T>(T extraValues) where T : class
    {
        return JObject.FromObject(extraValues);
    }
}