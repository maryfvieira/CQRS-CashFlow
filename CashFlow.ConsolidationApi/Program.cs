using CashFlow.Application.Dtos;
using Microsoft.OpenApi.Models;
using CashFlow.Application.Mapping;
using CashFlow.Application.Queries;
using CashFlow.Application.QueryHadlers;
using CashFlow.Application.Serialization;
using CashFlow.Application.Services;
using CashFlow.ConsolidationApi;
using CashFlow.ConsolidationApi.Extensions;
using CashFlow.ConsolidationApi.Filters;
using CashFlow.ConsolidationApi.Models;
using CashFlow.ConsolidationApi.Services;
using CashFlow.Domain.Aggregates;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Infrastructure.Persistence.Cache;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Settings;
using MediatR;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using MongoDbSettings = CashFlow.Infrastructure.Settings.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Configurações
//builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
//builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));

builder.Services.AddTransient(typeof(IRequestHandler<GetConsolidatedDataQuery, List<ConsolidateDetailsDto>>),
    typeof(GetConsolidatedDataQueryHandler));

builder.Services.AddScoped<IAggregateFactory<CashFlowAggregateRoot>, CashFlowAggregateFactory>();
builder.Services.AddSingleton(typeof(IAggregateDeserializer<>), typeof(BsonAggregateDeserializer<>));

// Databases
// Sql (MySql - Transaction database)
builder.Services.AddSqlReadModelPersistence();

// Cache (Redis - EventState - Snapshot)
builder.Services.AddCachePersistence(builder.Configuration);

builder.Services.AddAutoMapper(typeof(ReportProfile));

builder.Services
    .AddTransient<IConsolidationReportingService, ConsolidationReportingService>()
    .AddTransient<IReportingService, ReportingService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new BrazilianDateTimeJsonConverter());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Consolidation Api", Version = "v1" });
    // Customização do formato da data
    c.SchemaFilter<BrazilianDateSchemaFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add redirection from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapReportsEndpoints();
app.MapSettingsEndpoints();

await app.RunAsync();
