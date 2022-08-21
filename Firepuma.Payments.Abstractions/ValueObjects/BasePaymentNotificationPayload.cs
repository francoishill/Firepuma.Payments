using Newtonsoft.Json;
using TypeIndicatorConverter.Core.Attribute;
using TypeIndicatorConverter.NewtonsoftJson;

namespace Firepuma.Payments.Abstractions.ValueObjects;

[JsonConverter(typeof(TypeIndicatorConverter<BasePaymentNotificationPayload>))]
public abstract class BasePaymentNotificationPayload
{
    [TypeIndicator]
    // ReSharper disable once UnusedMember.Global
    public string ConcreteType => GetType().FullName;
}