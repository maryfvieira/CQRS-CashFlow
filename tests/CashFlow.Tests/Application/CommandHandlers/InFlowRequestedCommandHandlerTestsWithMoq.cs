using AutoFixture;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Commands;
using CashFlow.Application.Serialization;
using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Infrastructure.Settings;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using NSubstitute;

namespace CashFlow.Tests.Application.CommandHandlers;

public class InFlowRequestedCommandHandlerTestsWithMoq : InFlowCommandHandlerTestBase
{
    private readonly Guid _companyAccountId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private readonly string _formatedDate = $"{DateTime.UtcNow:yyyy-MM-dd}";
    private readonly decimal _balanceStartDate = 0;
    private readonly decimal _balanceEndDate = 100;
    
    [Fact]
    public async Task Handle_Should_Process_Request_When_Snapshot_Exists()
    {
        // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, CashFlowSnapshotGenerator.CompanyAccountId)
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);

        var mockAggregate = SetupAggregate(aggregateId, requestEvent, CashFlowSnapshotGenerator.BalanceStart, CashFlowSnapshotGenerator.BalanceEnd);

        // Configure mocks
        // MockObjects.AggregateFactory
        //     .Setup(f => f.Create(aggregateId))
        //     .Returns(mockAggregate.Object);

        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync(new CashFlowSnapshot { AggregateData = GetSnapshot() });
        
        string aggregateIdCaptured = null;
        IEnumerable<IDomainEvent> eventsCaptured = null;

        MockObjects.EventStore
            .Setup(s => s.AppendEventsAsync(
                It.IsAny<string>(), 
                It.IsAny<IEnumerable<IDomainEvent>>()))
            .Callback<string, IEnumerable<IDomainEvent>>((aggregateId, events) => 
            {
                aggregateIdCaptured = aggregateId; // Captura o primeiro parâmetro
                eventsCaptured = events;           // Captura o segundo parâmetro
            })
            .Returns(Task.CompletedTask); 

        MockObjects.Deserializer
            .Setup(d => d.Deserialize(It.IsAny<BsonDocument>()))
            .Returns(mockAggregate.Object);
        
        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .Returns(Task.CompletedTask);
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        var handler = CreateHandler(publisherFactory);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(requestEvent.TransactionId);
        
        aggregateIdCaptured.Should().Be(aggregateId);
        eventsCaptured.Should().BeOfType(typeof(List<IDomainEvent>));

        // Verify event store interactions
        // MockObjects.EventStore.Verify(
        //     e => e.AppendEventsAsync(
        //         aggregateId,
        //         It.Is<IList<IDomainEvent>>(events =>
        //             events.OfType<InFlowRequestedEvent>().Any(e =>
        //                 e.Amount == request.Amount &&
        //                 e.Description == request.Description) &&
        //             events.OfType<InFlowProcessedEvent>().Any(e =>
        //                 e.Amount == request.Amount &&
        //                 e.Description == request.Description))),
        //     Times.Once);

        // Verify snapshot persistence
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
            Times.Once);

        // Verify message publishing
        
        MockObjects.Publisher
            .Verify(x=>x.PublishAsync(It.Is<InFlowProcessedEvent>(e =>
                e.Amount == request.Amount &&
                e.Description == request.Description &&
                e.CompanyAccountId == request.CompanyAccountId)), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Process_Request_When_Last_Snapshot_Exists()
    {
        // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, CashFlowSnapshotGenerator.CompanyAccountId)
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);
        
        var mockAggregate = SetupAggregate(aggregateId, requestEvent, CashFlowSnapshotGenerator.BalanceStart, CashFlowSnapshotGenerator.BalanceEnd);

        // Configure mocks
        MockObjects.AggregateFactory
            .Setup(f => f.Create(aggregateId, mockAggregate.Object.BalanceStartDay, mockAggregate.Object.BalanceEndDay))
            .Returns(mockAggregate.Object);
        
        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync((CashFlowSnapshot?)null);
        
        MockObjects.SnapshotStore
            .Setup(s => s.GetLastSnapshotAsync(request.CompanyAccountId))
            .ReturnsAsync(CashFlowSnapshotGenerator.GenerateRandomSnapshot());
        
        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .Returns(Task.CompletedTask);
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        var handler = CreateHandler(publisherFactory);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(requestEvent.TransactionId);

        // Verify event store interactions
        MockObjects.EventStore.Verify(
            e => e.AppendEventsAsync(
                aggregateId,
                It.Is<IList<IDomainEvent>>(events =>
                    events.OfType<InFlowRequestedEvent>().Any(e =>
                        e.Amount == request.Amount &&
                        e.Description == request.Description) &&
                    events.OfType<InFlowProcessedEvent>().Any(e =>
                        e.Amount == request.Amount &&
                        e.Description == request.Description))),
            Times.Once);

        // Verify snapshot persistence
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
            Times.Once);

        // Verify message publishing
        // MockObjects.PublishEndpoint.Verify(
        //     x => x.Publish(It.Is<InFlowProcessedEvent>(e =>
        //             e.Amount == request.Amount &&
        //             e.Description == request.Description &&
        //             e.CompanyAccountId == request.CompanyAccountId),
        //         It.IsAny<CancellationToken>()),
        //     Times.Once);
        
        MockObjects.Publisher
            .Verify(x=>x.PublishAsync(It.Is<InFlowProcessedEvent>(e =>
                e.Amount == request.Amount &&
                e.Description == request.Description &&
                e.CompanyAccountId == request.CompanyAccountId)), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_Process_Request_When_No_Last_Snapshot_Exists()
    {
         // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, CashFlowSnapshotGenerator.CompanyAccountId)
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);
        
        var mockAggregate = SetupAggregate(aggregateId, requestEvent, 0, 0);
        
        // Configure mocks
        MockObjects.AggregateFactory
            .Setup(f => f.Create(aggregateId))
            .Returns(mockAggregate.Object);
        
        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync((CashFlowSnapshot?)null);
        
        MockObjects.SnapshotStore
            .Setup(s => s.GetLastSnapshotAsync(request.CompanyAccountId))
            .ReturnsAsync((CashFlowSnapshot?)null);
        
        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .Returns(Task.CompletedTask);
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        var handler = CreateHandler(publisherFactory);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(requestEvent.TransactionId);
        //Usando o Invocations
        
        // Verifica EventStore.AppendEventsAsync
        var eventStoreInvocations = MockObjects.EventStore.Invocations
            .Where(i => i.Method.Name == nameof(IEventStore.AppendEventsAsync))
            .ToList();

        eventStoreInvocations.Should().ContainSingle();
    
        var appendEventsArgs = eventStoreInvocations[0].Arguments;
        appendEventsArgs[0].Should().Be(aggregateId);
    
        var events = (IList<IDomainEvent>)appendEventsArgs[1];
        events.Should().HaveCount(2);
    
        events.OfType<InFlowRequestedEvent>().Should().ContainSingle(e => 
            e.Amount == request.Amount && 
            e.Description == request.Description);
    
        events.OfType<InFlowProcessedEvent>().Should().ContainSingle(e => 
            e.Amount == request.Amount && 
            e.Description == request.Description);

        // Verifica SnapshotStore.SaveSnapshotAsync
        var snapshotInvocations = MockObjects.SnapshotStore.Invocations
            .Where(i => i.Method.Name == nameof(ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>.SaveSnapshotAsync))
            .ToList();

        snapshotInvocations.Should().ContainSingle();

        // Verifica Publisher.PublishAsync
        var publisherInvocations = MockObjects.Publisher.Invocations
            .Where(i => i.Method.Name == nameof(Publisher<InFlowProcessedEvent>.PublishAsync))
            .ToList();

        publisherInvocations.Should().ContainSingle();
    
        var publishedEvent = (InFlowProcessedEvent)publisherInvocations[0].Arguments[0];
        publishedEvent.Amount.Should().Be(request.Amount);
        publishedEvent.Description.Should().Be(request.Description);
        publishedEvent.CompanyAccountId.Should().Be(request.CompanyAccountId);

        //usando o verify:
        // // Verify event store interactions
        // MockObjects.EventStore.Verify(
        //     e => e.AppendEventsAsync(
        //         aggregateId,
        //         It.Is<IList<IDomainEvent>>(events =>
        //             events.OfType<InFlowRequestedEvent>().Any(e =>
        //                 e.Amount == request.Amount &&
        //                 e.Description == request.Description) &&
        //             events.OfType<InFlowProcessedEvent>().Any(e =>
        //                 e.Amount == request.Amount &&
        //                 e.Description == request.Description))),
        //     Times.Once);
        //
        // // Verify snapshot persistence
        // MockObjects.SnapshotStore.Verify(
        //     s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
        //     Times.Once);
        //
        // // Verify message publishing
        // MockObjects.Publisher
        //     .Verify(x=>x.PublishAsync(It.Is<InFlowProcessedEvent>(e =>
        //         e.Amount == request.Amount &&
        //         e.Description == request.Description &&
        //         e.CompanyAccountId == request.CompanyAccountId)), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Rollback_When_AppendEventsAsync_Fails()
    {
        // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, Guid.NewGuid())
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);

        var mockAggregate = SetupAggregate(aggregateId, requestEvent, CashFlowSnapshotGenerator.BalanceStart, CashFlowSnapshotGenerator.BalanceEnd);

        // Configurar mocks para simular snapshot existente
        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync(new CashFlowSnapshot { AggregateData = GetSnapshot() });

        MockObjects.Deserializer
            .Setup(d => d.Deserialize(It.IsAny<BsonDocument>()))
            .Returns(mockAggregate.Object);
        
        // Simular falha no AppendEventsAsync
        var exceptionMessage = "Database connection failed";
        MockObjects.EventStore
            .Setup(e => e.AppendEventsAsync(aggregateId, It.IsAny<IList<IDomainEvent>>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));
       
        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .Returns(Task.CompletedTask);
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        
        var handler = CreateHandler(publisherFactory);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(Guid.Empty);

        // Verificar se a exceção foi logada corretamente
        // MockObjects.Logger.Verify(
        //     x => x.LogError("Erro ao executar crédito: {Message}", exceptionMessage),
        //     Times.Once);

        // Verificar se o AppendEventsAsync foi chamado
        MockObjects.EventStore.Verify(
            e => e.AppendEventsAsync(aggregateId, It.IsAny<IList<IDomainEvent>>()),
            Times.Once);
        
        MockObjects.EventStore.VerifyNoOtherCalls();

        // Verificar se NÃO houve tentativa de salvar snapshot
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
            Times.Never);

        // Verificar se NÃO houve publicação de evento
        MockObjects.Publisher
            .Verify(x=>x.PublishAsync(It.IsAny<InFlowProcessedEvent>()), Times.Never);

        // Verificar rollback de eventos
        MockObjects.EventStore.Verify(
            e => e.DeleteEventsAsync(aggregateId, It.IsAny<Guid>()),
            Times.Never); // Porque eventsAppended = false
    }
    
    [Fact]
    public async Task Handle_Should_Rollback_When_SaveSnapshotAsync_Fails()
    {
        // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, Guid.NewGuid())
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);

        var mockAggregate = SetupAggregate(aggregateId, requestEvent, CashFlowSnapshotGenerator.BalanceStart, CashFlowSnapshotGenerator.BalanceEnd);

        // Configurar mocks para simular snapshot existente
        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync(new CashFlowSnapshot { AggregateData = GetSnapshot() });

        MockObjects.Deserializer
            .Setup(d => d.Deserialize(It.IsAny<BsonDocument>()))
            .Returns(mockAggregate.Object);

        // Simular falha no SaveSnapshotAsync
        var exceptionMessage = "Snapshot storage failure";
        MockObjects.SnapshotStore
            .Setup(s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .Returns(Task.CompletedTask);
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        var handler = CreateHandler(publisherFactory);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(Guid.Empty);
        
        // Verificar que os eventos foram persistidos (antes do erro)
        MockObjects.EventStore.Verify(
            e => e.AppendEventsAsync(aggregateId, It.IsAny<IList<IDomainEvent>>()),
            Times.Once);

        // Verificar tentativa de salvar snapshot
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
            Times.Once);
        
        // Verificar rollback de eventos
        MockObjects.EventStore.Verify(
            e => e.DeleteEventsAsync(aggregateId, requestEvent.TransactionId),
            Times.Once); // eventsAppended = true
        
        MockObjects.Publisher
            .Verify(x=>x.PublishAsync(It.IsAny<InFlowProcessedEvent>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenPublishFails()
    {
        // Arrange
        var request = Fixture.Build<InFlowCommand>()
            .With(x => x.CompanyAccountId, CashFlowSnapshotGenerator.CompanyAccountId)
            .With(x => x.Amount, 100)
            .With(x => x.Description, "Credit from customer")
            .Create();

        var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var requestEvent = new InFlowRequestedEvent(
            request.CompanyAccountId,
            request.Amount,
            request.Description,
            DateTime.UtcNow);

        var mockAggregate = SetupAggregate(aggregateId, requestEvent, CashFlowSnapshotGenerator.BalanceStart, CashFlowSnapshotGenerator.BalanceEnd);
        var originalSnapshot = new CashFlowSnapshot
        {
            AggregateData = GetSnapshot()
        };

        // Configurar mocks
        MockObjects.SnapshotStore
            .Setup(s => s.GetSnapshotAsync(aggregateId))
            .ReturnsAsync(originalSnapshot);

        MockObjects.Deserializer
            .Setup(d => d.Deserialize(It.IsAny<BsonDocument>()))
            .Returns(mockAggregate.Object);

        // Configura o PublishAsync para lançar exceção
        MockObjects.Publisher
            .Setup(p => p.PublishAsync(It.IsAny<InFlowProcessedEvent>()))
            .ThrowsAsync(new InvalidOperationException("Event publishing failed"));
        
        Mock<IPublisherFactory> publisherFactory = new Mock<IPublisherFactory>();
        publisherFactory
            .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            .Returns(MockObjects.Publisher.Object);
        
        var handler = CreateHandler(publisherFactory);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        result.Should().Be(Guid.Empty);

        // Verificar se os eventos foram persistidos antes da falha
        MockObjects.EventStore.Verify(
            e => e.AppendEventsAsync(aggregateId, It.IsAny<IList<IDomainEvent>>()),
            Times.Once);

        // Verificar se o snapshot foi salvo antes da falha
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(It.IsAny<CashFlowAggregateRoot>()),
            Times.Once);

        // Verificar rollback dos eventos
        MockObjects.EventStore.Verify(
            e => e.DeleteEventsAsync(aggregateId, requestEvent.TransactionId),
            Times.Once);

        // Verificar restauração do snapshot original
        MockObjects.SnapshotStore.Verify(
            s => s.SaveSnapshotAsync(originalSnapshot),
            Times.Once);
        
        MockObjects.Publisher
            .Verify(x=>x.PublishAsync(It.IsAny<InFlowProcessedEvent>()), Times.Once);
        
    }

    // private Mock<CashFlowAggregateRoot> SetupAggregate(string id, InFlowRequestedEvent @event, decimal start, decimal end)
    // {
    //     var mock = new Mock<CashFlowAggregateRoot>();
    //     mock.SetupGet(a => a.Id).Returns(id);
    //     mock.SetupGet(a => a.Version).Returns(1);
    //     mock.SetupGet(a => a.EntityIdentifier).Returns(Guid.NewGuid());
    //     mock.Setup(a => a.GetSnapshot()).Returns(new CashFlowSnapshot
    //     {
    //         AggregateId = id,
    //         BalanceStart = start,
    //         BalanceEnd = end,
    //         EntityIdentifier = @event.CompanyAccountId,
    //         Date = DateTime.UtcNow,
    //         LastEventId = @event.EventId,
    //         Version = 1,
    //         AggregateData = GetSnapshot()
    //     });
    //
    //     mock.Setup(a => a.Apply(@event));
    //
    //     MockObjects.EventStore
    //         .Setup(e => e.AppendEventsAsync(mock.Object, It.IsAny<CancellationToken>()))
    //         .Returns(Task.CompletedTask);
    //
    //     MockObjects.SnapshotStore
    //         .Setup(s => s.SaveSnapshotAsync(It.IsAny<CashFlowSnapshot>(), It.IsAny<CancellationToken>()))
    //         .Returns(Task.CompletedTask);
    //
    //     return mock;
    // }
   

    private Mock<CashFlowAggregateRoot> SetupAggregate(
        string aggregateId,
        InFlowRequestedEvent requestEvent,
        decimal balanceStart,
        decimal balanceEnd)
    {
        var mockAggregate = new Mock<CashFlowAggregateRoot>(aggregateId);

        // Setup properties
        mockAggregate.Object.BalanceStartDay = balanceStart;
        mockAggregate.Object.BalanceEndDay = balanceEnd;

        // Setup methods
        mockAggregate
            .Setup(a => a.RequestCredit(
                requestEvent.Amount,
                requestEvent.Description,
                It.IsAny<DateTime>()))
            .ReturnsAsync(requestEvent.TransactionId);

        var domainEvents = new List<IDomainEvent>
        {
            requestEvent,
            new InFlowProcessedEvent(
                requestEvent.TransactionId,
                requestEvent.Amount,
                requestEvent.CompanyAccountId,
                requestEvent.Description,
                DateTime.UtcNow,
                balanceStart,
                balanceEnd)
        };

        mockAggregate
            .Setup(a => a.GetUncommittedEvents())
            .Returns(domainEvents);

        return mockAggregate;
    }

    private InFlowRequestCommandHandler CreateHandler(Mock<IPublisherFactory> publisherFactory) => new(
        MockObjects.EventStore.Object,
        MockObjects.SnapshotStore.Object,
        publisherFactory.Object,
        MockObjects.AggregateFactory.Object,
        MockObjects.Deserializer.Object,
        MockObjects.Logger.Object);

    private BsonDocument GetSnapshot()
    {
        
        return new BsonDocument
        {
            { "_id", _companyAccountId + "_" + _formatedDate},
            // { "_id", "3fa85f64-5717-4562-b3fc-2c963f66afa6_2025-04-24" },
            { "Version", 0 },
            { "EntityIdentifier", _companyAccountId.ToString("D") },
            { "Date", BsonDateTime.Create(DateTime.Parse("2025-04-24T23:03:04.878Z")) },
            { "BalanceStart", BsonDecimal128.Create(_balanceStartDate) },
            { "BalanceEnd", BsonDecimal128.Create(_balanceEndDate) },
            { "LastEventId", "c5e9e9c8-ed61-463e-9b54-e3f41a300036" },
            {
                "AggregateData", new BsonDocument
                {
                    { "AggregateId", _companyAccountId + "_" + _formatedDate },
                    { "Version", 16 },
                    {
                        "LastEventId",
                        new BsonBinaryData(Guid.Parse("11e41e3e-6cf4-4ce0-a2db-b26afa01c186"),
                            GuidRepresentation.Standard)
                    },
                    { "Date", BsonDateTime.Create(DateTime.Parse("2025-04-24T23:03:04.878062Z")) },
                    {
                        "CompanyAccountId",
                        new BsonBinaryData(_companyAccountId,
                            GuidRepresentation.Standard)
                    },
                    { "BalanceStartDay", BsonDecimal128.Create(_balanceStartDate) },
                    { "BalanceEndDay", BsonDecimal128.Create(_balanceEndDate) }
                }
            }
        };
    }
}