using CashFlow.Application.Dtos;
using MediatR;

namespace CashFlow.Application.Queries;

public record GetConsolidatedDataQuery(Guid CompanyAccountId, DateTime InitialDate, DateTime EndDate) : IRequest<List<ConsolidateDetailsDto>>; 