namespace Nik.Dbs.MongoDb;

public sealed class BsonEnumSerializer<T> : SerializerBase<T> where T : struct, Enum
{
    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var value = context.Reader.ReadString();
        return Enum.Parse<T>(value);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
    {
        context.Writer.WriteString(value.ToString());
    }
}