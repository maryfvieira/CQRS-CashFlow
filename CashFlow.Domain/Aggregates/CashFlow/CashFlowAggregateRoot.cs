using CashFlow.Domain.Events;

namespace CashFlow.Domain.Aggregates.CashFlow;

public class CashFlowAggregateRoot: AggregateRoot
{
    public Guid CompanyAccountId { get; init; }
    public decimal BalanceStartDay { get; set; }
    public decimal BalanceEndDay { get; set; }

    /// <summary>
    /// Default Constructor used by BsonDocument deserialization
    /// </summary>
    public CashFlowAggregateRoot() : base()
    {
    }
    
    public CashFlowAggregateRoot(string aggregateId)
    {
       AggregateId = aggregateId;
       CompanyAccountId = Guid.Parse(AggregateId.Split('_')[0]);
       //CreatedAt = date;
       //LastUpdateAt = date;
       //Date = DateTime.UtcNow;
    }
    
    public virtual Task<Guid> RequestCredit(decimal amount, string description, DateTime occurredAt)
    {
        var @event = new InFlowRequestedEvent(CompanyAccountId, amount, description, occurredAt);
        BalanceEndDay += amount;
        AddDomainEvent(@event);
        Apply(@event);
        
        return Task.FromResult(@event.TransactionId);
    }

    public Task ConsolidateCredit(Guid transactionId, decimal amount, string description, DateTime occurredAt)
    {
        var @event = new InFlowProcessedEvent(transactionId, amount, CompanyAccountId, description, occurredAt, BalanceStartDay, BalanceEndDay);
        AddDomainEvent(@event);
        Apply(@event);
        
        return Task.CompletedTask;
    }
    
    public Task<Guid> RequestDebit(decimal amount, string description, DateTime occurredAt)
    {
        var @event = new OutFlowRequestedEvent(CompanyAccountId, amount, description, occurredAt);
        BalanceEndDay -= amount;
        
        AddDomainEvent(@event);
        Apply(@event);
        
        return Task.FromResult(@event.TransactionId);
    }
    
    public Task ConsolidateDebit(Guid transactionId, decimal amount, string description, DateTime occurredAt)
    {
        var @event = new OutFlowProcessedEvent(transactionId, amount, CompanyAccountId, description, occurredAt, BalanceStartDay, BalanceEndDay);
        AddDomainEvent(@event);
        Apply(@event);
        return Task.CompletedTask;
    }
}