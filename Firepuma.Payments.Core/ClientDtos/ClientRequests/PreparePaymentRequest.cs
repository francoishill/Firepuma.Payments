using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json.Linq;

namespace Firepuma.Payments.Core.ClientDtos.ClientRequests;

public class PreparePaymentRequest
{
    public PaymentId PaymentId { get; set; }
    public JObject ExtraValues { get; set; }

    public bool TryCastExtraValuesToType<T>(out T extraValues, out string error) where T : class
    {
        try
        {
            extraValues = ExtraValues.ToObject<T>();
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            extraValues = null;
            error = exception.Message;
            return false;
        }
    }

    public static JObject CastToExtraValues<T>(T extraValues) where T : class
    {
        return JObject.FromObject(extraValues);
    }
}