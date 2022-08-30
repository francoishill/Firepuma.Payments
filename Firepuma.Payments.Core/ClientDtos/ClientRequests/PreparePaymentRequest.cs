using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json;

namespace Firepuma.Payments.Core.ClientDtos.ClientRequests;

public class PreparePaymentRequest
{
    public PaymentId PaymentId { get; set; }
    public object ExtraValues { get; set; }

    public bool TryCastExtraValuesToType<T>(out T extraValues, out string error) where T : class
    {
        try
        {
            //FIX: is there a better way than this?
            extraValues = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(ExtraValues));
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
}