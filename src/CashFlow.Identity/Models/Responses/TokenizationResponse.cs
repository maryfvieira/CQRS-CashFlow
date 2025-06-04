using System.Text.Json.Serialization;

namespace CashFlow.Identity.Models.Responses;

public record TokenizationResponse(
    [property: JsonPropertyName("token")] string? Token
): IResponse
{
    public TokenizationResponse() : this(string.Empty) { }
};
