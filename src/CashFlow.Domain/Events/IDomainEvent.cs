namespace CashFlow.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; set; }
    Guid TransactionId { get; set; }
    DateTime OccurredAt { get; set; }
    int Version { get; set; }
   
}