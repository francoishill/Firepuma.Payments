﻿using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Newtonsoft.Json.Linq;

namespace Firepuma.Payments.Core.Payments.Entities;

public class PaymentEntity : BaseEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public PaymentId PaymentId { get; set; }

    public PaymentStatus Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }

    public JObject ExtraValues { get; set; } = null!;

    // ReSharper disable once UnusedMember.Global
    public PaymentEntity()
    {
        // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
    }

    public PaymentEntity(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        JObject extraValues)
    {
        ApplicationId = applicationId;
        GatewayTypeId = gatewayTypeId;
        PaymentId = paymentId;
        ExtraValues = extraValues;

        Status = PaymentStatus.New;
    }

    public bool TryCastExtraValuesToType<T>(out T extraValues, out string? error) where T : class
    {
        try
        {
            extraValues = ExtraValues.ToObject<T>()!;

            if (extraValues == null)
            {
                throw new InvalidCastException($"Unable to cast ExtraValues to type {typeof(T).FullName}");
            }

            error = null;
            return true;
        }
        catch (Exception exception)
        {
            extraValues = null!;
            error = exception.Message;
            return false;
        }
    }

    public static JObject CastToExtraValues<T>(T extraValues) where T : class
    {
        return JObject.FromObject(extraValues);
    }
}