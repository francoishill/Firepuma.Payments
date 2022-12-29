using Newtonsoft.Json.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.Domain.Payments.ValueObjects;

public class PaymentApplicationConfig
{
    public required ClientApplicationId ApplicationId { get; set; }
    public required PaymentGatewayTypeId GatewayTypeId { get; set; }

    public required string ApplicationSecret { get; set; } = null!;

    public required JObject ExtraValues { get; set; } = null!; // can be used to store extra values specific to each payment gateway

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