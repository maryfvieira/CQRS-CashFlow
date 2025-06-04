using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace CashFlow.Application.Serialization;

public class BsonAggregateDeserializer<T> : IAggregateDeserializer<T>
{
    public T Deserialize(BsonDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return BsonSerializer.Deserialize<T>(document);
    }
}