using System.ComponentModel;
using System.Globalization;

namespace Firepuma.Payments.Abstractions.ValueObjects;

[System.Text.Json.Serialization.JsonConverter(typeof(PayFastPaymentIdSystemJsonConverter))]
[Newtonsoft.Json.JsonConverter(typeof(PayFastPaymentIdNewtonsoftJsonConverter))]
[TypeConverter(typeof(PayFastPaymentIdTypeConverter))]
public readonly struct PayFastPaymentId : IComparable<PayFastPaymentId>, IEquatable<PayFastPaymentId>
{
    public string Value { get; }

    public PayFastPaymentId(string value)
    {
        Value = value;
    }

    public bool Equals(PayFastPaymentId other) => Value?.Equals(other.Value) == true;
    public int CompareTo(PayFastPaymentId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is PayFastPaymentId other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(PayFastPaymentId a, PayFastPaymentId b) => a.CompareTo(b) == 0;
    public static bool operator !=(PayFastPaymentId a, PayFastPaymentId b) => !(a == b);

    private class PayFastPaymentIdSystemJsonConverter : System.Text.Json.Serialization.JsonConverter<PayFastPaymentId>
    {
        public override PayFastPaymentId Read(
            ref System.Text.Json.Utf8JsonReader reader,
            Type typeToConvert,
            System.Text.Json.JsonSerializerOptions options) =>
            new PayFastPaymentId(reader.GetString());

        public override void Write(
            System.Text.Json.Utf8JsonWriter writer,
            PayFastPaymentId val,
            System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(val.Value);
    }

    private class PayFastPaymentIdNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PayFastPaymentId);
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var id = (PayFastPaymentId)value;
            serializer.Serialize(writer, id.Value);
        }

        public override object ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            object existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return new PayFastPaymentId(str);
        }
    }

    private class PayFastPaymentIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            var stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                return new PayFastPaymentId(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}