using AutoFixture;
using AutoFixture.Xunit2;
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
using MongoDB.Bson.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Org.BouncyCastle.Crypto.Generators;

namespace CashFlow.Tests.Application.CommandHandlers
{
    public class InFlowRequestCommandHandlerTests
    {
        
        [Fact]
        public async Task Handle_Should_Process_Request_When_Snapshot_Exists()
        {
            var fixture = new Fixture();
            var request = fixture.Build<InFlowCommand>()
                .With(x => x.CompanyAccountId, Guid.NewGuid())
                .With(x => x.Amount, 100)
                .With(x => x.Description, "Credit from customer")
                .Create();

            var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";

            var requestEvent = new InFlowRequestedEvent(request.CompanyAccountId, request.Amount, request.Description,
                DateTime.UtcNow);

            var domainEvents = new List<IDomainEvent>
            {
                requestEvent,
                new InFlowProcessedEvent(requestEvent.TransactionId, request.Amount, request.CompanyAccountId, request.Description, DateTime.UtcNow, 200, 300)
            };

            var aggregateFactory = Substitute.For<IAggregateFactory<CashFlowAggregateRoot>>();
            
            var mockAggregate = Substitute.For<CashFlowAggregateRoot>(aggregateId);
            mockAggregate.BalanceStartDay = 200;
            mockAggregate.BalanceEndDay = 300;
            
            mockAggregate.RequestCredit(requestEvent.Amount, requestEvent.Description, Arg.Any<DateTime>())
                .Returns(Task.FromResult(requestEvent.TransactionId));
            
            mockAggregate.GetUncommittedEvents().Returns(domainEvents);
            
            // Fake snapshot data and deserialization
           
            var snapshot = new CashFlowSnapshot
            {
                AggregateData = GetSnapshot()
            };
            
            var eventStore = Substitute.For<IEventStore>();
            var snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();
            snapshotStore.GetSnapshotAsync(aggregateId).Returns(snapshot);
            //snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns(snapshot);
            
            //BsonSerializer.Deserialize<CashFlowAggregateRoot>(Arg.Any<BsonDocument>()).Returns(mockAggregate);
            
            var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());

            var publisher = Substitute.For<Publisher<InFlowProcessedEvent>>(Substitute.For<ILogger<Publisher<InFlowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
            var publisherFactory = Substitute.For<IPublisherFactory>();
            publisherFactory.CreatePublisher<InFlowProcessedEvent>().Returns(publisher);
            
            //var publisher = Substitute.For<Publisher<InFlowProcessedEvent>>();
            //var publisherFactory = Substitute.For<IPublisherFactory>();
            //publisherFactory.CreatePublisher<InFlowProcessedEvent>().Returns(publisher);

            var logger = Substitute.For<ILogger<InFlowRequestCommandHandler>>();
            
            var deserializer = Substitute.For<IAggregateDeserializer<CashFlowAggregateRoot>>();
            deserializer.Deserialize(Arg.Any<BsonDocument>()).Returns(mockAggregate);
            
            var handler = new InFlowRequestCommandHandler(eventStore, snapshotStore, publisherFactory, aggregateFactory, deserializer, logger);

            var result = await handler.Handle(request, CancellationToken.None);

            var processed = mockAggregate.GetUncommittedEvents().OfType<InFlowProcessedEvent>().First();
            
            result.Should().Be(requestEvent.TransactionId);
            
            await eventStore.Received(1).AppendEventsAsync(
                aggregateId,
                Arg.Is<IList<IDomainEvent>>(events =>
                    events.OfType<InFlowRequestedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description) &&
                    events.OfType<InFlowProcessedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description)));
            
            await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());

            await publisher.Received(1).PublishAsync(
                Arg.Is<InFlowProcessedEvent>(e =>
                    e.Amount == request.Amount &&
                    e.Description == request.Description &&
                    e.CompanyAccountId == request.CompanyAccountId));
        }
        
        [Fact]
        public async Task Handle_Should_Process_Request_When_Last_Snapshot_Exists()
        {
            var fixture = new Fixture();
            var request = fixture.Build<InFlowCommand>()
                .With(x => x.CompanyAccountId, Guid.NewGuid())
                .With(x => x.Amount, 100)
                .With(x => x.Description, "Credit from customer")
                .Create();

            var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
            
            var snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();
            snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot)null);
            
            var snapshot = new CashFlowSnapshot
            {
                BalanceStart=100,
                BalanceEnd=100,
                AggregateData = GetSnapshot()
            };
            
            snapshotStore.GetLastSnapshotAsync(request.CompanyAccountId).Returns(snapshot);
            
            var aggregateFactory = Substitute.For<IAggregateFactory<CashFlowAggregateRoot>>();
            
            var requestEvent = new InFlowRequestedEvent(request.CompanyAccountId, request.Amount, request.Description,
                DateTime.UtcNow);

            var domainEvents = new List<IDomainEvent>
            {
                requestEvent,
                new InFlowProcessedEvent(requestEvent.TransactionId, request.Amount, request.CompanyAccountId, request.Description, DateTime.UtcNow, 200, 300)
            };
            
            var mockAggregate = Substitute.For<CashFlowAggregateRoot>(aggregateId);
            
            mockAggregate.RequestCredit(requestEvent.Amount, requestEvent.Description, Arg.Any<DateTime>())
                .Returns(Task.FromResult(requestEvent.TransactionId));
            
            mockAggregate.GetUncommittedEvents().Returns(domainEvents);
            
            aggregateFactory = Substitute.For<IAggregateFactory<CashFlowAggregateRoot>>();
            aggregateFactory.Create(aggregateId, snapshot.BalanceStart, snapshot.BalanceEnd).Returns(mockAggregate);
            
            var eventStore = Substitute.For<IEventStore>();
            
            var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());

            var publisher = Substitute.For<Publisher<InFlowProcessedEvent>>(Substitute.For<ILogger<Publisher<InFlowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
            var publisherFactory = Substitute.For<IPublisherFactory>();
            publisherFactory.CreatePublisher<InFlowProcessedEvent>().Returns(publisher);
            
            var logger = Substitute.For<ILogger<InFlowRequestCommandHandler>>();
            
            var deserializer = Substitute.For<IAggregateDeserializer<CashFlowAggregateRoot>>();
            
            var handler = new InFlowRequestCommandHandler(eventStore, snapshotStore, publisherFactory, aggregateFactory, deserializer, logger);

            var result = await handler.Handle(request, CancellationToken.None);
            
            await eventStore.Received(1).AppendEventsAsync(
                aggregateId,
                Arg.Is<IList<IDomainEvent>>(events =>
                    events.OfType<InFlowRequestedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description) &&
                    events.OfType<InFlowProcessedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description && e.TransactionId == result)));
            
            await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());

            await publisher.Received(1).PublishAsync(
                Arg.Is<InFlowProcessedEvent>(e =>
                    e.Amount == request.Amount &&
                    e.Description == request.Description &&
                    e.CompanyAccountId == request.CompanyAccountId));
        }
        
        [Fact]
        public async Task Handle_Should_Process_Request_When_No_Last_Snapshot_Exists()
        {
            var fixture = new Fixture();
            var request = fixture.Build<InFlowCommand>()
                .With(x => x.CompanyAccountId, Guid.NewGuid())
                .With(x => x.Amount, 100)
                .With(x => x.Description, "Credit from customer")
                .Create();

            var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
            
            var snapshotStore = Substitute.For<ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot>>();
            snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot)null);
            
            // var snapshot = new CashFlowSnapshot
            // {
            //     BalanceStart=100,
            //     BalanceEnd=100,
            //     AggregateData = GetSnapshot()
            // };
            
            snapshotStore.GetLastSnapshotAsync(request.CompanyAccountId).Returns((CashFlowSnapshot)null);
            
            var aggregateFactory = Substitute.For<IAggregateFactory<CashFlowAggregateRoot>>();
            
            var requestEvent = new InFlowRequestedEvent(request.CompanyAccountId, request.Amount, request.Description,
                DateTime.UtcNow);

            var domainEvents = new List<IDomainEvent>
            {
                requestEvent,
                new InFlowProcessedEvent(requestEvent.TransactionId, request.Amount, request.CompanyAccountId, request.Description, DateTime.UtcNow, 200, 300)
            };
            
            var mockAggregate = Substitute.For<CashFlowAggregateRoot>(aggregateId);
            
            mockAggregate.RequestCredit(requestEvent.Amount, requestEvent.Description, Arg.Any<DateTime>())
                .Returns(Task.FromResult(requestEvent.TransactionId));
            
            mockAggregate.GetUncommittedEvents().Returns(domainEvents);
            
            aggregateFactory = Substitute.For<IAggregateFactory<CashFlowAggregateRoot>>();
            aggregateFactory.Create(aggregateId).Returns(mockAggregate);
            
            var eventStore = Substitute.For<IEventStore>();
            
            var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());

            var publisher = Substitute.For<Publisher<InFlowProcessedEvent>>(Substitute.For<ILogger<Publisher<InFlowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
            var publisherFactory = Substitute.For<IPublisherFactory>();
            publisherFactory.CreatePublisher<InFlowProcessedEvent>().Returns(publisher);
            
            var logger = Substitute.For<ILogger<InFlowRequestCommandHandler>>();
            
            var deserializer = Substitute.For<IAggregateDeserializer<CashFlowAggregateRoot>>();
            
            var handler = new InFlowRequestCommandHandler(eventStore, snapshotStore, publisherFactory, aggregateFactory, deserializer, logger);

            var result = await handler.Handle(request, CancellationToken.None);
            
            await eventStore.Received(1).AppendEventsAsync(
                aggregateId,
                Arg.Is<IList<IDomainEvent>>(events =>
                    events.OfType<InFlowRequestedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description) &&
                    events.OfType<InFlowProcessedEvent>().Any(e => e.Amount == request.Amount && e.Description == request.Description && e.TransactionId == result)));
            
            await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());

            await publisher.Received(1).PublishAsync(
                Arg.Is<InFlowProcessedEvent>(e =>
                    e.Amount == request.Amount &&
                    e.Description == request.Description &&
                    e.CompanyAccountId == request.CompanyAccountId));
        }

        private BsonDocument GetSnapshot()
        {
            return new BsonDocument
            {
                { "_id", "3fa85f64-5717-4562-b3fc-2c963f66afa6_2025-04-24" },
                { "Version", 0 },
                { "EntityIdentifier", "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                { "Date", BsonDateTime.Create(DateTime.Parse("2025-04-24T23:03:04.878Z")) },
                { "BalanceStart", BsonDecimal128.Create(0m) },
                { "BalanceEnd", BsonDecimal128.Create(8400.36m) },
                { "LastEventId", "c5e9e9c8-ed61-463e-9b54-e3f41a300036" },
                { "AggregateData", new BsonDocument
                    {
                        { "AggregateId", "3fa85f64-5717-4562-b3fc-2c963f66afa6_2025-04-24" },
                        { "Version", 16 },
                        { "LastEventId", new BsonBinaryData(Guid.Parse("11e41e3e-6cf4-4ce0-a2db-b26afa01c186"), GuidRepresentation.Standard) },
                        { "Date", BsonDateTime.Create(DateTime.Parse("2025-04-24T23:03:04.878062Z")) },
                        { "CompanyAccountId", new BsonBinaryData(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), GuidRepresentation.Standard) },
                        { "BalanceStartDay", BsonDecimal128.Create(0m) },
                        { "BalanceEndDay", BsonDecimal128.Create(8400.36m) }
                    }
                }
            };
        }
    }
}