namespace Nik.Dbs.MongoDb;

public class DateOnlySerializer : SerializerBase<DateOnly>
{
    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonReader = context.Reader;
        var dateTime = bsonReader.ReadDateTime();
        return DateOnly.FromDateTime(new DateTime(dateTime));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        var bsonWriter = context.Writer;
        bsonWriter.WriteDateTime(value.ToDateTime(TimeOnly.MinValue).Ticks);
    }
}