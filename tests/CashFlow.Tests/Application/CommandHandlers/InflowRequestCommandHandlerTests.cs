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
using NSubstitute;

namespace CashFlow.Tests.Application.CommandHandlers;

public class InflowRequestCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly IEventStore _eventStore;
    private readonly ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> _snapshotStore;
    private readonly ILogger<InFlowRequestCommandHandler> _logger;
    private readonly Publisher<InFlowProcessedEvent> _publisher;
    private readonly IPublisherFactory _publisherFactory;
    private readonly InFlowRequestCommandHandler _handler;

    public InflowRequestCommandHandlerTests()
    {
        _fixture = new Fixture();
        _eventStore = Substitute.For<IEventStore>();
        _snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();
        _logger = Substitute.For<ILogger<InFlowRequestCommandHandler>>();

        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());

        _publisher = Substitute.For<Publisher<InFlowProcessedEvent>>(Substitute.For<ILogger<Publisher<InFlowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        _publisherFactory = Substitute.For<IPublisherFactory>();
        _publisherFactory.CreatePublisher<InFlowProcessedEvent>().Returns(_publisher);
//corrigir
        _handler = new InFlowRequestCommandHandler(_eventStore, _snapshotStore, _publisherFactory, null,null, _logger);
    }

    [Fact]
    public async Task Handle_ShouldProcessCreditAndReturnTransactionId()
    {
        // Arrange
        var command = _fixture.Build<InFlowCommand>()
                              .With(x => x.CompanyAccountId, Guid.NewGuid())
                              .With(c => c.Amount, 100.0m)
                              .With(x => x.Description, "Deposit from Mary")
                              .Create();

        var snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();

        snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        snapshotStore.GetLastSnapshotAsync(command.CompanyAccountId).Returns((CashFlowSnapshot?)null);

        var handler = new InFlowRequestCommandHandler(_eventStore, snapshotStore, _publisherFactory, null, null, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        await _eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Is<IEnumerable<IDomainEvent>>(e => e.Any()));
        await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await _publisher.Received().PublishAsync(Arg.Any<InFlowProcessedEvent>());
    }

    [Fact(Skip = "Refatorar publisher para testar")]
    public async Task Handle_ShouldProcessCredit_WhenSnapshotIsNull()
    {
        // Arrange
        var command = _fixture.Create<InFlowCommand>();

        _snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        _snapshotStore.GetLastSnapshotAsync(command.CompanyAccountId).Returns((CashFlowSnapshot?)null);

        InFlowProcessedEvent? publishedEvent = null;
        _publisher.PublishAsync(Arg.Do<InFlowProcessedEvent>(e => publishedEvent = e)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);

        await _eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<IDomainEvent>>());
        await _snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await _publisher.Received().PublishAsync(Arg.Any<InFlowProcessedEvent>());

        publishedEvent.Should().NotBeNull();
        publishedEvent!.Amount.Should().Be(command.Amount);
    }

    [Fact(Skip = "Refatorar publisher para testar")]
    public async Task Handle_ShouldAppendEvents_AndPublishProcessedEvent_WhenSnapshotNotFound()
    {
        // Arrange
        var command = _fixture.Create<InFlowCommand>();
        var aggregateId = $"{command.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";

        _snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        _snapshotStore.GetLastSnapshotAsync(Arg.Any<Guid>()).Returns((CashFlowSnapshot?)null);

        var publishedEvents = new List<InFlowProcessedEvent>();

        _publisher.PublishAsync(Arg.Do<InFlowProcessedEvent>(e =>
        {
            publishedEvents.Add(e);
        })).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        await _eventStore.Received(1).AppendEventsAsync(Arg.Is<string>(id => id.StartsWith(command.CompanyAccountId.ToString())), Arg.Any<IEnumerable<IDomainEvent>>());
        await _snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        _publisher.Received().PublishAsync(Arg.Any<InFlowProcessedEvent>());

        result.Should().NotBe(Guid.Empty);
        publishedEvents.Should().ContainSingle();
    }
}
