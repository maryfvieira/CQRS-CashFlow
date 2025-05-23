namespace CashFlow.Domain.Entities;

public sealed record UserRole(
    Guid UserId,
    Guid RoleId)
{
    public User User { get; init; }
    public Role Role { get; init; }
}