using CashFlow.Domain.Documents;
using MongoDB.Bson;

namespace CashFlow.Tests.Application.CommandHandlers;

public class CashFlowSnapshotGenerator
{
    public static readonly Guid CompanyAccountId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    public static readonly string FormatedDate = $"{DateTime.UtcNow:yyyy-MM-dd}";
    public static readonly decimal BalanceStart = 0;
    public static readonly decimal BalanceEnd = 200;
    
    public static CashFlowSnapshot GenerateRandomSnapshot()
    {
        var random = new Random();
            
        return new CashFlowSnapshot
        {
            AggregateId = CompanyAccountId + "_" + FormatedDate, // ID no formato string
            Version = random.Next(1, 100), // Versão entre 1 e 100
            EntityIdentifier = CompanyAccountId, // Novo GUID
            Date = DateTime.UtcNow, // Data atual em UTC
            BalanceStart = 0, // Valor decimal aleatório
            BalanceEnd = 200, // Valor decimal aleatório
            LastEventId = Guid.NewGuid(), // Novo GUID
            AggregateData = GenerateRandomBsonDocument() // Documento BSON aleatório
        };
    }
    private static BsonDocument GenerateRandomBsonDocument()
    {
        return new BsonDocument
        {
            { "timestamp", DateTime.UtcNow },
            { "eventType", "RandomEvent" },
            { "value", new Random().Next(1000, 5000) },
            { "isProcessed", new Random().Next(2) == 1 } // Valor booleano aleatório
        };
    }
}