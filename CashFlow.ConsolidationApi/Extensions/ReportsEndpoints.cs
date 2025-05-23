using System.Net;
using CashFlow.ConsolidationApi.Models.Requests;
using CashFlow.ConsolidationApi.Services;
using CashFlow.CrossCutting.Abstractions;
using CashFlow.CrossCutting.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;

namespace CashFlow.ConsolidationApi.Extensions;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/consolidation/report", 
            async ([FromBody] ReportDailyRequest request, [FromServices] IConsolidationReportingService service) =>
            {
                var result = await service.GetConsolidatedReport(request);
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
                context.Response.StatusCode = result.StatusCode;
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    }
}