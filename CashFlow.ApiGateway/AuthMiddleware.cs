using System.Text;
using Newtonsoft.Json;

namespace CashFlow.ApiGateway;

using System.Net;

public class AuthMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthMiddleware(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Token não fornecido");
                return;
            }

            var client = _httpClientFactory.CreateClient("IdentityClient");
            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var json = JsonConvert.SerializeObject(token);
            var tokenRequest = new StringContent(json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("", tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = "application/json";
                var content = await response.Content.ReadAsStringAsync();
                await context.Response.WriteAsync(content != string.Empty ? content : "Token inválido");
                //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //await context.Response.WriteAsync("Token inválido");
                return;
            }

            await next(context);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        } 
    }
}
