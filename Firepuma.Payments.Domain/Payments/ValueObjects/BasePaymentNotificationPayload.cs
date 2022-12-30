using TypeIndicatorConverter.Core.Attribute;

namespace Firepuma.Payments.Domain.Payments.ValueObjects;

[System.Text.Json.Serialization.JsonConverter(typeof(TypeIndicatorConverter.TextJson.TypeIndicatorConverter<BasePaymentNotificationPayload>))]
public class BasePaymentNotificationPayload
{
    [TypeIndicator]
    // ReSharper disable once UnusedMember.Global
    public string? ConcreteType => GetType().FullName;
}