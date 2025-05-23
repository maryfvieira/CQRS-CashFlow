using CashFlow.CrossCutting.Abstractions;
using CashFlow.CrossCutting.Abstractions.Extensions;
using CashFlow.TransactionsApi.Models.Requests;
using CashFlow.TransactionsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.TransactionsApi.Extensions;

public static class DailyCashManagementEndpoints
{
    public static void MapDailyCashManagementEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/transaction/outflow/",
            async ([FromBody] OutFlowRequest request, [FromServices] ICashManagementService service) => 
            {
                var result = await service.CreateOutFlowRequest(request);
                return result.ToIResult();
            });
        
        app.MapPost("/api/v1/transaction/inflow/", 
            async ([FromBody] InFlowRequest request, [FromServices] ICashManagementService service) =>
            {
                var result = await service.CreateInFlowRequest(request);
                return result.ToIResult();
            });
        
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var result = HttpResult<object>.InternalServerError(new Error(ex.Message));
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    }
}