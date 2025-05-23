namespace CashFlow.Domain.Entities;

public class ConsolidateDetails
{
    public Guid CompanyAccountId { get; set; }
    public DateTime Date { get; set;}
    public string Description { get; set;}
    public OperationType OperationType { get; set;}
    public decimal Amount { get; set;}
    public decimal BalanceStartDay { get; set;}
    public decimal Balance { get; set;}
}
    