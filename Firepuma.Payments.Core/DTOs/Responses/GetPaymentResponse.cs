using Firepuma.Payments.Core.ValueObjects;

// ReSharper disable CollectionNeverUpdated.Global

namespace Firepuma.Payments.Core.DTOs.Responses;

public class GetPaymentResponse
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public Dictionary<string, object> ExtraValues { get; set; }
}