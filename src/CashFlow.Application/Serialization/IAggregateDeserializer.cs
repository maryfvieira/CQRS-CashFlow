using MongoDB.Bson;

namespace CashFlow.Application.Serialization;

public interface IAggregateDeserializer<TAggregate>
{
    TAggregate Deserialize(BsonDocument doc);
}