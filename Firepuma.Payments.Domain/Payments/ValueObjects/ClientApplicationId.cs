using System.ComponentModel;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Firepuma.Payments.Domain.Payments.ValueObjects;

[System.Text.Json.Serialization.JsonConverter(typeof(ClientApplicationIdSystemJsonConverter))]
[Newtonsoft.Json.JsonConverter(typeof(ClientApplicationIdNewtonsoftJsonConverter))]
[TypeConverter(typeof(ClientApplicationIdTypeConverter))]
[BsonSerializer(typeof(ClientApplicationIdMongoSerializer))]
public readonly struct ClientApplicationId : IComparable<ClientApplicationId>, IEquatable<ClientApplicationId>
{
    public string Value { get; }

    public ClientApplicationId(string value)
    {
        Value = value;
    }

    public bool Equals(ClientApplicationId other) => Value?.Equals(other.Value) == true;
    public int CompareTo(ClientApplicationId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is ClientApplicationId other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(ClientApplicationId a, ClientApplicationId b) => a.CompareTo(b) == 0;
    public static bool operator !=(ClientApplicationId a, ClientApplicationId b) => !(a == b);

    private class ClientApplicationIdSystemJsonConverter : System.Text.Json.Serialization.JsonConverter<ClientApplicationId>
    {
        public override ClientApplicationId Read(
            ref System.Text.Json.Utf8JsonReader reader,
            Type typeToConvert,
            System.Text.Json.JsonSerializerOptions options) =>
            new ClientApplicationId(reader.GetString()!);

        public override void Write(
            System.Text.Json.Utf8JsonWriter writer,
            ClientApplicationId val,
            System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(val.Value);
    }

    private class ClientApplicationIdNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ClientApplicationId);
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var id = (ClientApplicationId?)value;
            serializer.Serialize(writer, id?.Value);
        }

        public override object ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            object? existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return new ClientApplicationId(str!);
        }
    }

    private class ClientApplicationIdTypeConverter : TypeConverter
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
                return new ClientApplicationId(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
    
    private class ClientApplicationIdMongoSerializer : SerializerBase<ClientApplicationId>
    {
        public override ClientApplicationId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return new ClientApplicationId(context.Reader.ReadString());
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            ClientApplicationId value)
        {
            context.Writer.WriteString(value.Value);
        }
    }

    static ClientApplicationId()
    {
        BsonTypeMapper.RegisterCustomTypeMapper(typeof(ClientApplicationId), new CustomClientApplicationIdMapper());
    }

    private class CustomClientApplicationIdMapper : ICustomBsonTypeMapper
    {
        public bool TryMapToBsonValue(object value, out BsonValue bsonValue)
        {
            bsonValue = new BsonString(((ClientApplicationId)value).Value);
            return true;
        }
    }
}