using AutoFixture;
using CashFlow.Application.EventHandlers;
using CashFlow.Application.Services;
using CashFlow.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.EventHandlers;

public class InFlowProcessedEventHandlerTests
{
    private readonly IFixture _fixture;
    private readonly ITransactionsService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<InFlowProcessedEventHandler> _logger;
    private readonly InFlowProcessedEventHandler _handler;

    public InFlowProcessedEventHandlerTests()
    {
        _fixture = new Fixture();
        _service = Substitute.For<ITransactionsService>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<InFlowProcessedEventHandler>>();

        _handler = new InFlowProcessedEventHandler(_service, _mediator, _logger);
    }

    [Fact]
    public async Task HandleAsync_Should_Call_CreditAsync_And_Return_TransactionId()
    {
        // Arrange
        var creditEvent = _fixture.Create<InFlowProcessedEvent>();
        var expectedTransactionId = Guid.NewGuid();

        _service.InFlowAsync(
            creditEvent.CompanyAccountId,
            creditEvent.Amount,
            creditEvent.Description,
            creditEvent.OccurredAt,
            creditEvent.BalanceStartDay,
            creditEvent.BalanceEndDay)
            .Returns(expectedTransactionId);

        // Act
        var result = await _handler.HandleAsync(creditEvent);

        // Assert
        await _service.Received(1).InFlowAsync(
            creditEvent.CompanyAccountId,
            creditEvent.Amount,
            creditEvent.Description,
            creditEvent.OccurredAt,
            creditEvent.BalanceStartDay,
            creditEvent.BalanceEndDay);

        result.Should().Be(expectedTransactionId);

        _logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Processed credit transaction")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
