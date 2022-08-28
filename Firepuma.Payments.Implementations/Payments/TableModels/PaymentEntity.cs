using Firepuma.Payments.Abstractions.Entities;
using Firepuma.Payments.Abstractions.ValueObjects;
using Newtonsoft.Json;

namespace Firepuma.Payments.Implementations.Payments.TableModels;

public class PaymentEntity : BaseEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public Dictionary<string, object> ExtraValues { get; set; }

    // ReSharper disable once UnusedMember.Global
    public PaymentEntity()
    {
        // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
    }

    public PaymentEntity(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        Dictionary<string, object> extraValues)
    {
        ApplicationId = applicationId;
        GatewayTypeId = gatewayTypeId;
        PaymentId = paymentId;
        ExtraValues = extraValues;

        Status = PaymentStatus.New;
    }

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