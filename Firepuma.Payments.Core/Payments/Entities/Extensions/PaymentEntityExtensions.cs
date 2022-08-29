using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.Core.Payments.Entities.Extensions;

public static class PaymentEntityExtensions
{
    public static void SetStatus(this PaymentEntity entity, PaymentStatus status)
    {
        entity.Status = status;
        entity.StatusChangedOn = DateTime.UtcNow;
    }
}