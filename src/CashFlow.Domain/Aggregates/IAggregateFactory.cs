namespace CashFlow.Domain.Aggregates;

public interface IAggregateFactory<TAggregateRoot>
{
    TAggregateRoot Create(string aggregateId);
    TAggregateRoot Create(string aggregateId, decimal balanceStart, decimal balanceEnd);
}
