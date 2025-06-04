using Microsoft.OpenApi.Models;
using CashFlow.Application.Services;
using CashFlow.Identity.Extensions;
using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Services;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Persistence.Sql.DapperTypeHandlers;
using CashFlow.Infrastructure.Settings;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
ILogger logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// Configurações
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

logger.LogInformation("1. Configuração básica do JWT (apenas para geração/validação)");
var jwtSettings = new JwtSettings();
builder.Configuration.Bind(jwtSettings);
builder.Configuration.Bind(JwtSettings.SectionName, jwtSettings);
builder.Services.AddSingleton(jwtSettings);

//var jwtSettings = builder.Configuration.GetSection("Jwt");
// builder.Services.AddSingleton(new JwtSettings(
//     jwtSettings["Key"]!,
//     jwtSettings["Issuer"]!,
//     jwtSettings["Audience"]!
// ));

logger.LogInformation("2. Configuraçāo básica do banco de dados");
builder.Services.AddSqlAuthModelPersistence();

logger.LogInformation("3. Serviços de autenticação (sem middleware de autenticação)");
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, CashFlow.Application.Services.AuthService>();
builder.Services.AddScoped<ITokenizationService, TokenizationService>();

logger.LogInformation("4. Configuração do Swagger (sem segurança)");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Token Service API", Version = "v1" });
});

var app = builder.Build();

logger.LogInformation("5. Pipeline mínimo");
app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();

logger.LogInformation("6. Mapeamento de endpoints públicos");
app.MapAuthEndpoints(); 
app.MapUserEndpoints();

app.Run();
