using System.ComponentModel;
using System.Globalization;

namespace Firepuma.Payments.Core.Payments.ValueObjects;

[System.Text.Json.Serialization.JsonConverter(typeof(PaymentGatewayTypeIdSystemJsonConverter))]
[Newtonsoft.Json.JsonConverter(typeof(PaymentGatewayTypeIdNewtonsoftJsonConverter))]
[TypeConverter(typeof(PaymentGatewayTypeIdTypeConverter))]
public readonly struct PaymentGatewayTypeId : IComparable<PaymentGatewayTypeId>, IEquatable<PaymentGatewayTypeId>
{
    public string Value { get; }

    public PaymentGatewayTypeId(string value)
    {
        Value = value;
    }

    public bool Equals(PaymentGatewayTypeId other) => Value?.Equals(other.Value) == true;
    public int CompareTo(PaymentGatewayTypeId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is PaymentGatewayTypeId other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(PaymentGatewayTypeId a, PaymentGatewayTypeId b) => a.CompareTo(b) == 0;
    public static bool operator !=(PaymentGatewayTypeId a, PaymentGatewayTypeId b) => !(a == b);

    private class PaymentGatewayTypeIdSystemJsonConverter : System.Text.Json.Serialization.JsonConverter<PaymentGatewayTypeId>
    {
        public override PaymentGatewayTypeId Read(
            ref System.Text.Json.Utf8JsonReader reader,
            Type typeToConvert,
            System.Text.Json.JsonSerializerOptions options) =>
            new PaymentGatewayTypeId(reader.GetString()!);

        public override void Write(
            System.Text.Json.Utf8JsonWriter writer,
            PaymentGatewayTypeId val,
            System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(val.Value);
    }

    private class PaymentGatewayTypeIdNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PaymentGatewayTypeId);
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var id = (PaymentGatewayTypeId?)value;
            serializer.Serialize(writer, id?.Value);
        }

        public override object ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            object? existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return new PaymentGatewayTypeId(str!);
        }
    }

    private class PaymentGatewayTypeIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            var stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                return new PaymentGatewayTypeId(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}