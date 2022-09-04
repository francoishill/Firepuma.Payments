using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;

[JsonConverter(typeof(StringEnumConverter))]
public enum ServiceAlertType
{
    NewDeadLetteredMessages,
}