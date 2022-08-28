using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Firepuma.Payments.Abstractions.ValueObjects;

[JsonConverter(typeof(StringEnumConverter))]
public enum PaymentStatus
{
    New,
    Succeeded,
    Cancelled,
}