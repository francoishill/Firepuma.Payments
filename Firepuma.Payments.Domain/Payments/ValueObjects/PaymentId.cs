using System.ComponentModel;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Firepuma.Payments.Domain.Payments.ValueObjects;

[System.Text.Json.Serialization.JsonConverter(typeof(PaymentIdSystemJsonConverter))]
[Newtonsoft.Json.JsonConverter(typeof(PaymentIdNewtonsoftJsonConverter))]
[TypeConverter(typeof(PaymentIdTypeConverter))]
[BsonSerializer(typeof(PaymentIdMongoSerializer))]
public readonly struct PaymentId : IComparable<PaymentId>, IEquatable<PaymentId>
{
    public string Value { get; }

    public PaymentId(string value)
    {
        Value = value;
    }

    public static PaymentId GenerateNew() => new($"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid().ToString()}");

    public bool Equals(PaymentId other) => Value?.Equals(other.Value) == true;
    public int CompareTo(PaymentId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is PaymentId other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(PaymentId a, PaymentId b) => a.CompareTo(b) == 0;
    public static bool operator !=(PaymentId a, PaymentId b) => !(a == b);

    private class PaymentIdSystemJsonConverter : System.Text.Json.Serialization.JsonConverter<PaymentId>
    {
        public override PaymentId Read(
            ref System.Text.Json.Utf8JsonReader reader,
            Type typeToConvert,
            System.Text.Json.JsonSerializerOptions options) =>
            new PaymentId(reader.GetString()!);

        public override void Write(
            System.Text.Json.Utf8JsonWriter writer,
            PaymentId val,
            System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(val.Value);
    }

    private class PaymentIdNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PaymentId);
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var id = (PaymentId?)value;
            serializer.Serialize(writer, id?.Value);
        }

        public override object ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            object? existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return new PaymentId(str!);
        }
    }

    private class PaymentIdTypeConverter : TypeConverter
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
            var stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                return new PaymentId(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    private class PaymentIdMongoSerializer : SerializerBase<PaymentId>
    {
        public override PaymentId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return new PaymentId(context.Reader.ReadString());
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            PaymentId value)
        {
            context.Writer.WriteString(value.Value);
        }
    }

    static PaymentId()
    {
        BsonTypeMapper.RegisterCustomTypeMapper(typeof(PaymentId), new CustomPaymentIdMapper());
    }

    private class CustomPaymentIdMapper : ICustomBsonTypeMapper
    {
        public bool TryMapToBsonValue(object value, out BsonValue bsonValue)
        {
            bsonValue = new BsonString(((PaymentId)value).Value);
            return true;
        }
    }
}