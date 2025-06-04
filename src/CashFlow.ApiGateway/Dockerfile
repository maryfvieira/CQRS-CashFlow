FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CashFlow.ApiGateway/CashFlow.ApiGateway.csproj", "src/CashFlow.ApiGateway/"]
COPY ["src/CashFlow.Application/CashFlow.Application.csproj", "src/CashFlow.Application/"]
COPY ["src/CashFlow.CrossCutting/CashFlow.CrossCutting.csproj", "src/CashFlow.CrossCutting/"]
COPY ["src/CashFlow.Domain/CashFlow.Domain.csproj", "src/CashFlow.Domain/"]
COPY ["src/CashFlow.Infrastructure/CashFlow.Infrastructure.csproj", "src/CashFlow.Infrastructure/"]
RUN dotnet restore "src/CashFlow.ApiGateway/CashFlow.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/CashFlow.ApiGateway"
RUN dotnet build "CashFlow.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CashFlow.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.ApiGateway.dll"]
