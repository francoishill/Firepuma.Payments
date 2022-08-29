using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Infrastructure.Payments.TableModels.Extensions;

public static class PaymentTableEntityExtensions
{
    public static void SetStatus(this PaymentEntity entity, PaymentStatus status)
    {
        entity.Status = status;
        entity.StatusChangedOn = DateTime.UtcNow;
    }
}