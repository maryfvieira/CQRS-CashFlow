using AutoFixture;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using CashFlow.Application.QueryHadlers;
using CashFlow.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CashFlow.Tests.Application.QueryHandlers;

public class GetConsolidatedDataQueryHandlerTests
{
    private readonly IFixture _fixture;

    public GetConsolidatedDataQueryHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Should_ThrowException_When_Handle_GetConsolidationDataQuery_Given_InvalidCompanyAccountId()
    {
        // Arrange
        var companyAccountId = Guid.Parse("fac6d145-c77d-4f17-9035-1c2b686062c9");

        var query = _fixture.Build<GetConsolidatedDataQuery>()
            .With(x => x.CompanyAccountId, companyAccountId)
            .Create();

        var reportingService = Substitute.For<IReportingService>();
        var logger = Substitute.For<ILogger<GetConsolidatedDataQueryHandler>>();

        reportingService.GetConsolidatedReportsAsync(query.CompanyAccountId, query.InitialDate, query.EndDate).Throws<Exception>();

        var commandHandler = new GetConsolidatedDataQueryHandler(reportingService, logger);

        // Act
        // Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            _ = await commandHandler.Handle(query, CancellationToken.None);
        });
    }

    [Fact]
    public async Task Should_Return_ConsolidationDetails_When_Handle_GetConsolidationDataQuery_Given_ValidCompanyAccountId()
    {
        // Arrange
        var companyAccountId = Guid.Parse("fac6d145-c77d-4f17-9035-1c2b686062c6");

        var query = _fixture.Build<GetConsolidatedDataQuery>()
            .With(x => x.CompanyAccountId, companyAccountId)
            .Create();

        var consolidation = _fixture.Create<ConsolidateDetailsDto>();
        List<ConsolidateDetailsDto> consolidationDetailsDto = [ consolidation ];

        var reportingService = Substitute.For<IReportingService>();
        var logger = Substitute.For<ILogger<GetConsolidatedDataQueryHandler>>();

        reportingService.GetConsolidatedReportsAsync(query.CompanyAccountId, query.InitialDate, query.EndDate).Returns(consolidationDetailsDto);

        var commandHandler = new GetConsolidatedDataQueryHandler(reportingService, logger);

        // Act
        var consolidateDetails = await commandHandler.Handle(query, CancellationToken.None);

        // Assert
        consolidateDetails.Should().HaveCount(1);
        consolidateDetails.Any(cd => cd.CompanyAccountId == companyAccountId).Should().BeTrue();
    }
}
