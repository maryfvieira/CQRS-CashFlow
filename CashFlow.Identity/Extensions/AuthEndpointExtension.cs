using System.Net;
using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Models.Responses;
using CashFlow.Identity.Services;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;

namespace CashFlow.Identity.Extensions;

public static class AuthEndpointsExtension
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/identity/login", 
            async ([FromBody] AuthRequest credentials, ITokenizationService tokenizationService) =>
            {
               
                if (!MiniValidator.TryValidate(credentials, out var errors))
                {
                    return await Task.FromResult(Result<TokenizationResponse>
                        .BadRequest(errors.Values.SelectMany(e => e)));
                }
                
                var result = await tokenizationService.GenerateToken(credentials);
                
                // return await Task.FromResult(Result<TokenizationResponse>
                //     .Created(new TokenizationResponse(null, errors.Values.SelectMany(e => e))));

                return result;

                // return Result<TokenizationResponse>.Created(result).ToIResult();
                // return result.ToIResult();
            });
        
        app.MapPost("/api/v1/identity/validateToken", 
            async ([FromBody] string token, ITokenizationService tokenizationService) =>
            {
                var result = await tokenizationService.ValidateToken(token);
                return result;
            });
    }

    // private static IResult ToIResult<T>(this Result<T> result)
    // {
    //     if (result.IsSuccess)
    //     {
    //         return result.StatusCode switch
    //         {
    //             (int)HttpStatusCode.Created => Results.Created("", result.Details),
    //             (int)HttpStatusCode.NoContent => Results.NoContent(),
    //             _ => Results.Ok(result.Details)
    //         };
    //     }
    //     
    //     return result.StatusCode switch
    //     {
    //         (int)HttpStatusCode.BadRequest => Results.BadRequest(result.Error),
    //         (int)HttpStatusCode.Unauthorized => Results.Unauthorized(),
    //         (int)HttpStatusCode.NotFound => Results.NotFound(result.Error),
    //         (int)HttpStatusCode.Conflict => Results.Conflict(result.Error),
    //         _ => Results.StatusCode(result.StatusCode)
    //     };
    // }
}