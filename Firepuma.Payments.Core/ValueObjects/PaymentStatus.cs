using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Firepuma.Payments.Core.ValueObjects;

[JsonConverter(typeof(StringEnumConverter))]
public enum PaymentStatus
{
    New,
    Succeeded,
    Cancelled,
}