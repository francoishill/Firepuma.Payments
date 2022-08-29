using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;

// ReSharper disable CollectionNeverUpdated.Global

namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public class GetPaymentResponse
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public Dictionary<string, object> ExtraValues { get; set; }
}