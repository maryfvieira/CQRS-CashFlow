namespace CashFlow.ApiGateway;

public class Identity
{
    public string Url { get; set; }
}
public class Settings
{
    public Identity Identity { get; set; }
}