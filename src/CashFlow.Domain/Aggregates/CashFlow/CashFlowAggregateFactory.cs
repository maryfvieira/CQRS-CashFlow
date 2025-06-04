namespace CashFlow.Domain.Aggregates.CashFlow;

public class CashFlowAggregateFactory : IAggregateFactory<CashFlowAggregateRoot>
{
    public CashFlowAggregateRoot Create(string aggregateId)
    {
        return new CashFlowAggregateRoot(aggregateId)
        {
            BalanceStartDay = 0,
            BalanceEndDay = 0
        };
    }
    
    public CashFlowAggregateRoot Create(string aggregateId, decimal balanceStart, decimal balanceEnd)
    {
        return new CashFlowAggregateRoot(aggregateId)
        {
            BalanceStartDay = balanceStart,
            BalanceEndDay = balanceEnd
        };
    }
}