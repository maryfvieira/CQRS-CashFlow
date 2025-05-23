using AutoFixture;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Commands;
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
using NSubstitute;

namespace CashFlow.Tests.Application.CommandHandlers;

public class OutFlowRequestCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IEventStore _eventStore;
    private readonly ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> _snapshotStore;
    private readonly IPublisherFactory _publisherFactory;
    private readonly ILogger<OutFlowRequestCommandHandler> _logger;
    private readonly Publisher<OutFlowProcessedEvent> _publisher;

    private readonly OutFlowRequestCommandHandler _handler;

    public OutFlowRequestCommandHandlerTests()
    {
        _fixture = new Fixture();
        _eventStore = Substitute.For<IEventStore>();
        _snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();
        _publisherFactory = Substitute.For<IPublisherFactory>();
        _logger = Substitute.For<ILogger<OutFlowRequestCommandHandler>>();

        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());

        _publisher = Substitute.For<Publisher<OutFlowProcessedEvent>>(Substitute.For<ILogger<Publisher<OutFlowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        _publisherFactory.CreatePublisher<OutFlowProcessedEvent>().Returns(_publisher);

        _handler = new OutFlowRequestCommandHandler(_eventStore, _snapshotStore, _publisherFactory, _logger);
    }

    [Fact]
    public async Task Handle_ShouldProcessDebitTransaction_AndPublishEvents()
    {
        // Arrange
        var command = _fixture.Create<OutFlowCommand>();
        var aggregateId = $"{command.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";

        var aggregate = new CashFlowAggregateRoot(aggregateId)
        {
            BalanceStartDay = 1000,
            BalanceEndDay = 1000
        };

        // simula snapshot null, mas tem o último snapshot (sem serialização)
        _snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        _snapshotStore.GetLastSnapshotAsync(Arg.Any<Guid>()).Returns(new CashFlowSnapshot
        {
            AggregateId = aggregate.AggregateId,
            AggregateData = aggregate.ToBsonDocument(aggregate.GetType()),
            EntityIdentifier = aggregate.CompanyAccountId,
            BalanceEnd = 1000
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<IDomainEvent>>());
        await _snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await _publisher.Received().PublishAsync(Arg.Any<OutFlowProcessedEvent>());
        result.Should().NotBe(Guid.Empty);
    }
}
