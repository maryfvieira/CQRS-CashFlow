namespace CashFlow.Infrastructure.Settings;

public record JwtSettings()
{
    public const string SectionName = "Jwt";
    
    public string Audience { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Key { get; init; } = default!;
    public int ClockSkewInMinutes { get; init; } = default!;
    public int ExpiryInHours { get; set; } = default!;
}