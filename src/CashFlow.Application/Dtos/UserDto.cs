using CashFlow.CrossCutting;

namespace CashFlow.Application.Dtos;

// public sealed record UserDto(Guid Id, string Username, string Email, string Password, List<RoleTypes> Roles);

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string Password,
    List<RoleTypes> Roles
)
{
    public UserDto() : this(default, "", "", "", new List<RoleTypes>()) { }
};