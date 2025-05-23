using CashFlow.Application.Dtos;
using CashFlow.ConsolidationApi.Models.Requests;
using CashFlow.CrossCutting.Abstractions;

namespace CashFlow.ConsolidationApi.Services;

public interface IConsolidationReportingService
{
    public Task<HttpResult<List<ConsolidateDetailsDto>>> GetConsolidatedReport(ReportDailyRequest request);
}