using Newtonsoft.Json;
using TypeIndicatorConverter.Core.Attribute;
using TypeIndicatorConverter.NewtonsoftJson;

namespace Firepuma.Payments.Abstractions.ValueObjects;

[JsonConverter(typeof(TypeIndicatorConverter<BasePaymentNotificationPayload>))]
public abstract class BasePaymentNotificationPayload
{
    [TypeIndicator]
    public string ConcreteType => GetType().FullName;
}