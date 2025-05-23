using Microsoft.AspNetCore.Http;
using System.Net;

namespace CashFlow.CrossCutting.Abstractions.Extensions;

public static class HttpResultExtensions
{
    public static IResult ToIResult<T>(this HttpResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                (int)HttpStatusCode.OK => Results.Ok(result.Value),
                (int)HttpStatusCode.Created => Results.Created("", result.Value),
                (int)HttpStatusCode.NoContent => Results.NoContent(),
                _ => Results.Ok(result.Value)
            };
        }
        
        return result.StatusCode switch
        {
            (int)HttpStatusCode.BadRequest => Results.BadRequest(result.Error),
            (int)HttpStatusCode.Unauthorized => Results.Unauthorized(),
            (int)HttpStatusCode.NotFound => Results.NotFound(result.Error),
            (int)HttpStatusCode.Conflict => Results.Conflict(result.Error),
            _ => Results.StatusCode(result.StatusCode)
        };
    }
}