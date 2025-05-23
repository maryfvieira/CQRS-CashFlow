using System.Text;

namespace CashFlow.ApiGateway;

public class TokenValidationHandler : DelegatingHandler
{
    private readonly Settings _settings;

    public TokenValidationHandler(Settings settings)
    {
        _settings = settings;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.RequestUri = new Uri(_settings.Identity.Url); 
        request.Method = HttpMethod.Post;
        
        var token = request.Headers.Authorization?.Parameter;

        var content = new StringContent($"{{ \"token\": \"{token}\" }}", Encoding.UTF8, "application/json");

        var newRequest = new HttpRequestMessage(HttpMethod.Post, _settings.Identity.Url)
        {
            Content = content
        };

        return await base.SendAsync(newRequest, cancellationToken);
    }
}
