using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json.Linq;

// ReSharper disable CollectionNeverUpdated.Global

namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public class GetPaymentResponse
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public object ExtraValues { get; set; }
}