namespace Firepuma.Payments.Domain.Payments.ValueObjects;

[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum PaymentStatus
{
    New,
    Succeeded,
    Cancelled,
}