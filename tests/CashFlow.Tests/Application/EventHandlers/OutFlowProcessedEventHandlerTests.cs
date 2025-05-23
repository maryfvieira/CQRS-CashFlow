using AutoFixture;
using CashFlow.Application.EventHandlers;
using CashFlow.Application.Services;
using CashFlow.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.EventHandlers;

public class OutFlowProcessedEventHandlerTests
{
    private readonly IFixture _fixture;
    private readonly ITransactionsService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<OutFlowProcessedEventHandler> _logger;
    private readonly OutFlowProcessedEventHandler _handler;

    public OutFlowProcessedEventHandlerTests()
    {
        _fixture = new Fixture();
        _service = Substitute.For<ITransactionsService>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<OutFlowProcessedEventHandler>>();

        _handler = new OutFlowProcessedEventHandler(_service, _mediator, _logger);
    }

    [Fact]
    public async Task HandleAsync_Should_Call_DebitAsync_And_Return_TransactionId()
    {
        // Arrange
        var debitEvent = _fixture.Create<OutFlowProcessedEvent>();
        var expectedTransactionId = Guid.NewGuid();

        _service.OutFlowAsync(
            debitEvent.CompanyAccountId,
            debitEvent.Amount,
            debitEvent.Description,
            debitEvent.OccurredAt,
            debitEvent.BalanceStartDay,
            debitEvent.BalanceEndDay)
            .Returns(expectedTransactionId);

        // Act
        var result = await _handler.HandleAsync(debitEvent);

        // Assert
        await _service.Received(1).OutFlowAsync(
            debitEvent.CompanyAccountId,
            debitEvent.Amount,
            debitEvent.Description,
            debitEvent.OccurredAt,
            debitEvent.BalanceStartDay,
            debitEvent.BalanceEndDay);

        result.Should().Be(expectedTransactionId);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Processed debit transaction")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
