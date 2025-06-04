using CashFlow.Application.Commands;
using CashFlow.Application.Services;
using Microsoft.Extensions.Logging;
using CashFlow.Domain.Events;
using MediatR;

namespace CashFlow.Application.EventHandlers;

public class OutFlowProcessedEventHandler : IEventHandler<OutFlowProcessedEvent, Guid>
{
    private readonly ITransactionsService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<OutFlowProcessedEventHandler> _logger;

    public OutFlowProcessedEventHandler(ITransactionsService service, IMediator mediator, ILogger<OutFlowProcessedEventHandler> logger)
    {
        _service = service;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(OutFlowProcessedEvent @event)
    {
        _logger.LogDebug("Processed debit transaction: {EntityIdentifier} {EventData}", 
            @event.CompanyAccountId, 
            System.Text.Json.JsonSerializer.Serialize(@event));

        var transactionId = await _service.OutFlowAsync(
            @event.CompanyAccountId,
            @event.Amount,
            @event.Description,
            @event.OccurredAt,
            @event.BalanceStartDay,
            @event.BalanceEndDay);
        
        // Emite o comando de forma assíncrona (não espera a conclusão)
        await _mediator.Send(new ReprocessConsolidatedReportsCommand(
            @event.CompanyAccountId,
            @event.OccurredAt));

        return transactionId;
    }
}