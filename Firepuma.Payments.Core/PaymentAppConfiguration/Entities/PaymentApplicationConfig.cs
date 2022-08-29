using Firepuma.Payments.Core.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.Core.PaymentAppConfiguration.Entities;

public class PaymentApplicationConfig : BaseEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public string ApplicationSecret { get; set; }

    public Dictionary<string, object> ExtraValues { get; set; } // can be used to store extra values specific to each payment gateway

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

    public static Dictionary<string, object> CastToExtraValues<T>(T extraValues) where T : class
    {
        //FIX: is there a better way than this?
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(extraValues));
    }
}