﻿namespace CashFlow.Domain.Events;

public sealed record OutFlowProcessedEvent(Guid TransactionId, decimal Amount, Guid CompanyAccountId, string Description, DateTime OccurredAt, decimal BalanceStartDay, decimal BalanceEndDay) : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; } = TransactionId;
    public DateTime OccurredAt { get; set; } = OccurredAt;
    public int Version { get; set; }
}
