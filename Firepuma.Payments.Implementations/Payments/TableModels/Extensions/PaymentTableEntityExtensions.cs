﻿using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Implementations.Payments.TableModels.Extensions;

public static class PaymentTableEntityExtensions
{
    public static void SetStatus(this PaymentEntity entity, PaymentStatus status)
    {
        entity.Status = status;
        entity.StatusChangedOn = DateTime.UtcNow;
    }
}