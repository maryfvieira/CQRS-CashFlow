using CashFlow.CrossCutting;

namespace CashFlow.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public RoleTypes Name  { get; set;}
}