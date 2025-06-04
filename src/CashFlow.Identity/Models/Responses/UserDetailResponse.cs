using System.Text.Json.Serialization;

namespace CashFlow.Identity.Models.Responses;

public record UserDetailResponse(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("userName")] string? UserName,
    [property: JsonPropertyName("roles")] List<string>? Roles,
    [property: JsonPropertyName("Errors")] IEnumerable<string>? Errors
): IResponse
{
    public UserDetailResponse() : this(string.Empty, string.Empty, new List<string>(), new List<string>()) { }
};
