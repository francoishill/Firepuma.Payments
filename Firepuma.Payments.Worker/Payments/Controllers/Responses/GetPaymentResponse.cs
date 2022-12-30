using AutoMapper;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;

// ReSharper disable CollectionNeverUpdated.Global

namespace Firepuma.Payments.Worker.Payments.Controllers.Responses;

[AutoMap(typeof(PaymentEntity))]
public class GetPaymentResponse
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public object? ExtraValues { get; set; }
}