using CashFlow.Domain.Events;

namespace CashFlow.Infrastructure.Persistence.NoSql.Interfaces;

public interface IEventStore
{
    Task AppendEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events);
    Task DeleteEventsAsync(string aggregateId, Guid transactionId);
}

