using CashFlow.Application.Commands;
using CashFlow.Application.Serialization;
using CashFlow.Domain.Aggregates;
using MediatR;
using MongoDB.Bson.Serialization;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.CommandHandlers
{
    public class InFlowRequestCommandHandler : IRequestHandler<InFlowCommand, Guid>
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> _snapshotStore;
        private readonly Publisher<InFlowProcessedEvent> _publisher;
        private readonly IAggregateFactory<CashFlowAggregateRoot> _aggregateFactory;
        private readonly ILogger<InFlowRequestCommandHandler> _logger;
        private readonly IAggregateDeserializer<CashFlowAggregateRoot> _deserializer;

        public InFlowRequestCommandHandler(IEventStore eventStore,
            ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> snapshotStore,
            IPublisherFactory publisherFactory,
            IAggregateFactory<CashFlowAggregateRoot> aggregateFactory,
            IAggregateDeserializer<CashFlowAggregateRoot> deserializer,
            ILogger<InFlowRequestCommandHandler> logger)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _deserializer = deserializer;
            _publisher = publisherFactory.CreatePublisher<InFlowProcessedEvent>();
            _aggregateFactory = aggregateFactory;
            _logger = logger;
        }

        public async Task<Guid> Handle(InFlowCommand request, CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.Empty;
            bool eventsAppended = false;
            bool snapshotSaved = false;
            bool hasSnapshot = false;
            CashFlowAggregateRoot aggregate = null;
            CashFlowSnapshot snapshot = null;
            
            try
            {
                var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
                snapshot = await _snapshotStore.GetSnapshotAsync(aggregateId);
                
                if (snapshot != null)
                {
                    //aggregate = JsonConvert.DeserializeObject<CashFlowAggregateRoot>(snapshot.AggregateData)!;
                    //aggregate = BsonSerializer.Deserialize<CashFlowAggregateRoot>(snapshot.AggregateData);
                    aggregate = _deserializer.Deserialize(snapshot.AggregateData);
                    hasSnapshot = true;
                }
                else
                {
                    snapshot ??= await _snapshotStore.GetLastSnapshotAsync(request.CompanyAccountId);
                    if (snapshot != null)
                        aggregate = _aggregateFactory.Create(aggregateId, snapshot.BalanceStart, snapshot.BalanceEnd);
                    else 
                        aggregate = _aggregateFactory.Create(aggregateId);
                }

                var eventDate = DateTime.UtcNow;

                transactionId = await aggregate.RequestCredit(request.Amount, request.Description, eventDate);
                await aggregate.ConsolidateCredit(transactionId, request.Amount, request.Description, eventDate);

                var @events = aggregate.GetUncommittedEvents();

                await _eventStore.AppendEventsAsync(aggregate.AggregateId, @events);
                eventsAppended = true;
                
                await _snapshotStore.SaveSnapshotAsync(aggregate);
                snapshotSaved = true;

                foreach (var @event in @events.Where(p => typeof(InFlowProcessedEvent) == p.GetType()))
                {
                    try
                    {
                        await _publisher.PublishAsync((InFlowProcessedEvent)@event);
                    
                        _logger.LogDebug("Evento {EventName} publicado {EventData}", nameof(InFlowProcessedEvent), System.Text.Json.JsonSerializer.Serialize((InFlowProcessedEvent)@event));
                        
                        return transactionId;
                    }
                    catch (Exception pubEx)
                    {
                        _logger.LogError("Erro ao executar crédito: {Message}", pubEx.Message);
                        await _eventStore.DeleteEventsAsync(aggregate.AggregateId, transactionId);
                        if (snapshot != null)
                            await _snapshotStore.SaveSnapshotAsync(snapshot);
                        else
                            await _snapshotStore.DeleteSnapshotAsync(aggregate.AggregateId);
                        
                        return Guid.Empty;
                    }
                }
                
                return transactionId;
            }
            catch (Exception ex)
            {
                if (eventsAppended)
                {
                    await _eventStore.DeleteEventsAsync(aggregate?.AggregateId, transactionId);
                }

                if (snapshotSaved)
                {
                    if (snapshot != null)
                        await _snapshotStore.SaveSnapshotAsync(snapshot);
                    else
                        await _snapshotStore.DeleteSnapshotAsync(aggregate?.AggregateId);
                }
                
                _logger.LogError("Erro ao executar crédito: {Message}", ex.Message);

                return Guid.Empty;
            }
        }
    }
}