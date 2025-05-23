using AutoFixture;
using AutoFixture.AutoMoq;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Serialization;
using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Infrastructure.Settings;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;

namespace CashFlow.Tests.Application.CommandHandlers;

public abstract class InFlowCommandHandlerTestBase
{
    protected readonly IFixture Fixture;
    protected readonly MockContainer MockObjects; // Renomeado para evitar conflito

    protected InFlowCommandHandlerTestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
        MockObjects = new MockContainer();
        
        //Fixture.Inject(MockObjects.PublisherFactory.Object);
        Fixture.Inject(MockObjects.Logger.Object);
    }

    protected class MockContainer // Classe interna renomeada
    {
        public Mock<IEventStore> EventStore { get; } = new();
        public Mock<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>> SnapshotStore { get; } = new();
        //public Mock<IPublisherFactory> PublisherFactory { get; } = new();
        public Mock<IAggregateFactory<CashFlowAggregateRoot>> AggregateFactory { get; } = new();
        public Mock<IAggregateDeserializer<CashFlowAggregateRoot>> Deserializer { get; } = new();
        public Mock<ILogger<InFlowRequestCommandHandler>> Logger { get; } = new();
        public Mock<IPublishEndpoint> PublishEndpoint { get; } = new();
        public Mock<Publisher<InFlowProcessedEvent>> Publisher { get; } = new();
        
        public MockContainer()
        {
            var appSettings = new AppSettings(
                new AppSettingsFixture().Configuration, 
                Mock.Of<ILogger<AppSettings>>());

            Publisher = new Mock<Publisher<InFlowProcessedEvent>>(
                MockBehavior.Strict,
                Mock.Of<ILogger<Publisher<InFlowProcessedEvent>>>(),
                appSettings,
                PublishEndpoint.Object)
                ;
            
            // PublisherFactory
            //     .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            //     .Returns(Publisher.Object);
            
            // Configura o PublisherFactory para retornar um Publisher real com mock do IPublishEndpoint
            // PublisherFactory
            //     .Setup(f => f.CreatePublisher<InFlowProcessedEvent>())
            //     .Returns(new Publisher<InFlowProcessedEvent>(
            //         Mock.Of<ILogger<Publisher<InFlowProcessedEvent>>>(),
            //         appSettings,
            //         PublishEndpoint.Object));
        }
    }

    protected virtual BsonDocument GetSnapshot() => new()
    {
        ["BalanceStartDay"] = 200,
        ["BalanceEndDay"] = 300,
        ["CompanyAccountId"] = Guid.NewGuid().ToString()
    };
}


