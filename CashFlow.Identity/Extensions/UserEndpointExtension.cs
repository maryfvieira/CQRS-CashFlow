using System.Net;
using AutoMapper;
using CashFlow.Application.Dtos;
using CashFlow.Application.Services;
using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;
using Error = CashFlow.Identity.Models.Responses.Error;

namespace CashFlow.Identity.Extensions;

public static class UserEndpointsExtension
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/identity/user/register", 
            async ([FromBody] UserRegistrationRequest user, IUserService userService, IMapper mapper) =>
            {
                try
                {
                    if (!MiniValidator.TryValidate(user, out var errors))
                    {
                        return Result<UserRegistrationResponse>
                            .BadRequest(errors.Values.SelectMany(e => e)).ToIResult();
                        
                    }
                
                    var userDto = mapper.Map<UserDto>(user);
                    var result = await userService.RegisterUserAsync(userDto);
                    
                    return Result<UserRegistrationResponse>
                        .Created(new UserRegistrationResponse(result, null)).ToIResult();

                }
                catch (Exception ex)
                {
                    return Result<UserRegistrationResponse>
                        .InternalServerError(new List<string> { "Ocorreu um erro ao generar o token: " + ex.Message }).ToIResult();
                }
            });
    }

    // private static IResult ToIResult<T>(this Models.Responses.Result<T> result)
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