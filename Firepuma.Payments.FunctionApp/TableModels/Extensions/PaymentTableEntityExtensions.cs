using System;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.TableModels.Extensions;

public static class PaymentTableEntityExtensions
{
    public static void SetStatus(this IPaymentTableEntity entity, PaymentStatus status)
    {
        entity.Status = status.ToString();
        entity.StatusChangedOn = DateTime.UtcNow;
    }
}