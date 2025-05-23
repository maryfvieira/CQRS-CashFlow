using MassTransit;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Commands;
using CashFlow.Application.Extensions;
using CashFlow.Application.Serialization;
using CashFlow.Application.Services;
using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.Cache;
using CashFlow.Infrastructure.Persistence.NoSql;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Settings;
using CashFlow.TransactionsApi.Extensions;
using CashFlow.TransactionsApi.Services;
using MediatR;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using MongoDbSettings = CashFlow.Infrastructure.Settings.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();
builder.Services.AddSingleton(typeof(IAggregateDeserializer<>), typeof(BsonAggregateDeserializer<>));
builder.Services.AddScoped<IAggregateFactory<CashFlowAggregateRoot>, CashFlowAggregateFactory>();

// 2. Configuração do MediatR (apenas command handlers)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));

builder.Services
    .AddTransient(typeof(IRequestHandler<InFlowCommand, Guid>), typeof(InFlowRequestCommandHandler))
    .AddTransient(typeof(IRequestHandler<OutFlowCommand, Guid>), typeof(OutFlowRequestCommandHandler));

var serviceProvider = builder.Services.BuildServiceProvider();

// NoSql (MongoDb - EventStore - Event Sourcing)
builder.Services.AddNoSqlPersistence(serviceProvider.GetRequiredService<IAppSettings>());

// Cache (Redis - EventState - Snapshot)
builder.Services.AddCachePersistence(builder.Configuration);

// MassTransit Configuration 
builder.AddMassTransitRabbitMqPublisher();

builder.Services.AddSingleton<IPublisherFactory, PublisherFactory>();

// CashFlow.Application services
builder.Services
    .AddTransient<ICashManagementService, CashManagementService>();

// Configuração global para evitar problemas com DateTime
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.String));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Transactions API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add redirection from root to Swagger
//app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapDailyCashManagementEndpoints();
app.MapSettingsEndpoints();

await app.RunAsync();