using CashFlow.ApiGateway;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();
builder.Configuration.Bind(settings);
builder.Configuration.Bind(nameof(Settings), settings);
builder.Services.AddSingleton(settings);

// Configuração do YARP
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// HttpClient que chama o Identity
builder.Services.AddHttpClient("IdentityClient", client =>
{
    client.BaseAddress = new Uri(settings.Identity.Url);
});
    //.AddHttpMessageHandler<TokenValidationHandler>();

// Handlers e Middlewares
//builder.Services.AddTransient<TokenValidationHandler>();
builder.Services.AddTransient<AuthMiddleware>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT com o prefixo 'Bearer '",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de autenticação
app.UseMiddleware<AuthMiddleware>();

// Roteamento YARP
app.MapReverseProxy();

app.Run();