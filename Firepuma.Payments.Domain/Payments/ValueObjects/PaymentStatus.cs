using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Firepuma.Payments.Domain.Payments.ValueObjects;

[JsonConverter(typeof(StringEnumConverter))]
public enum PaymentStatus
{
    New,
    Succeeded,
    Cancelled,
}