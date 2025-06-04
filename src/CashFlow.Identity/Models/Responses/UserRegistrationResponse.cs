using System.Text.Json.Serialization;

namespace CashFlow.Identity.Models.Responses;

public record UserRegistrationResponse(
    [property: JsonPropertyName("UserId")] Guid? Id,
    [property: JsonPropertyName("Errors")] IEnumerable<string>? Errors
): IResponse
{
    public UserRegistrationResponse() : this(Guid.Empty, new List<string>()) { }
};