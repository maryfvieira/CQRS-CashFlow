using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Identity.Models.Responses;

public class Result<T> where T: IResponse
{
    public bool IsSuccess { get; }
    public T? Details { get; }
    public IEnumerable<string>? Errors { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, T details,  int statusCode)
    {
        IsSuccess = isSuccess;
        Details = details;
        StatusCode = statusCode;
    }

    private Result(bool isSuccess, int statusCode, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Errors = errors;
    }
    
    public static Result<T> Success(T value) => new Result<T>(true, value, (int)HttpStatusCode.OK);
    public static Result<T> Created(T value) => new Result<T>(true, value,(int)HttpStatusCode.Created);
    public static Result<T> Accepted(T value) => new Result<T>(true, value, (int)HttpStatusCode.Accepted);
    public static Result<T> BadRequest(IEnumerable<string> errors) => new Result<T>(false,  (int)HttpStatusCode.BadRequest, errors);
    public static Result<T> Unauthorized(IEnumerable<string> errors) => new Result<T>(false, (int)HttpStatusCode.Unauthorized, errors);
    public static Result<T> Forbidden(IEnumerable<string> errors) => new Result<T>(false, (int)HttpStatusCode.Forbidden, errors);
    public static Result<T> InternalServerError(IEnumerable<string> errors) => new Result<T>(false, (int)HttpStatusCode.InternalServerError, errors);
    public static Result<T> NoContent() => new Result<T>(true, default, (int)HttpStatusCode.NoContent);
}

public static class ResultExtensions
{
    public static IResult ToIResult<T>(this Result<T> result) where T : IResponse
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                (int)HttpStatusCode.Created => Results.Created("", result.Details),
                (int)HttpStatusCode.NoContent => Results.NoContent(),
                _ => Results.Ok(result.Details)
            };
        }

        // Tratamento de erros com Problem Details (RFC 7807)
        var problemDetails = new ProblemDetails
        {
            Title = "Ocorreu um erro na requisição",
            Status = result.StatusCode,
            Extensions = { ["errors"] = result.Errors }
        };

        return result.StatusCode switch
        {
            (int)HttpStatusCode.BadRequest => Results.BadRequest(problemDetails),
            (int)HttpStatusCode.Unauthorized => Results.Unauthorized(),
            (int)HttpStatusCode.Forbidden => Results.Forbid(),
            (int)HttpStatusCode.NotFound => Results.NotFound(problemDetails),
            (int)HttpStatusCode.InternalServerError => Results.Problem(problemDetails),
            _ => Results.StatusCode(result.StatusCode)
        };
    }
}