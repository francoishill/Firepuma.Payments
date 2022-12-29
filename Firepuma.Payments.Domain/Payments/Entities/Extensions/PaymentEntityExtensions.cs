using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Entities.Extensions;

public static class PaymentEntityExtensions
{
    public static void SetStatus(this PaymentEntity entity, PaymentStatus status)
    {
        entity.Status = status;
        entity.StatusChangedOn = DateTime.UtcNow;
    }
}