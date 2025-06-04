FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CashFlow.TransactionsExecutor/CashFlow.TransactionsExecutor.csproj", "src/CashFlow.TransactionsExecutor/"]
COPY ["src/CashFlow.Application/CashFlow.Application.csproj", "src/CashFlow.Application/"]
COPY ["src/CashFlow.CrossCutting/CashFlow.CrossCutting.csproj", "src/CashFlow.CrossCutting/"]
COPY ["src/CashFlow.Domain/CashFlow.Domain.csproj", "src/CashFlow.Domain/"]
COPY ["src/CashFlow.Infrastructure/CashFlow.Infrastructure.csproj", "src/CashFlow.Infrastructure/"]
RUN dotnet restore "src/CashFlow.TransactionsExecutor/CashFlow.TransactionsExecutor.csproj"
COPY . .
WORKDIR "/src/src/CashFlow.TransactionsExecutor"
RUN dotnet build "CashFlow.TransactionsExecutor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CashFlow.TransactionsExecutor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.TransactionsExecutor.dll"]
