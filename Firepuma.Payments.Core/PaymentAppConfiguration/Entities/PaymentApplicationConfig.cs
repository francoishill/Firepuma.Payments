using Firepuma.Payments.Core.Infrastructure.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.Core.PaymentAppConfiguration.Entities;

public class PaymentApplicationConfig : BaseEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public string ApplicationSecret { get; set; }

    public JObject ExtraValues { get; set; } // can be used to store extra values specific to each payment gateway

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