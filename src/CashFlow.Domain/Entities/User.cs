using CashFlow.CrossCutting;

namespace CashFlow.Domain.Entities;

public sealed record User(
    Guid Id,
    string Email,
    string Username,
    string Password,
    DateTime CreatedAt)
{
    // Propriedade de navegação para roles (não mapeada diretamente pelo Dapper)
    public List<UserRole> UserRoles { get; set; } = new();
    public string Password { get; init; } = Password;

    // Helper para obter apenas os nomes das roles
    public IEnumerable<RoleTypes> Roles => UserRoles.Select(ur => ur.Role.Name);
}