FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CashFlow.TransactionsApi/CashFlow.TransactionsApi.csproj", "src/CashFlow.TransactionsApi/"]
COPY ["src/CashFlow.Application/CashFlow.Application.csproj", "src/CashFlow.Application/"]
COPY ["src/CashFlow.CrossCutting/CashFlow.CrossCutting.csproj", "src/CashFlow.CrossCutting/"]
COPY ["src/CashFlow.Domain/CashFlow.Domain.csproj", "src/CashFlow.Domain/"]
COPY ["src/CashFlow.Infrastructure/CashFlow.Infrastructure.csproj", "src/CashFlow.Infrastructure/"]
RUN dotnet restore "src/CashFlow.TransactionsApi/CashFlow.TransactionsApi.csproj"
COPY . .
WORKDIR "/src/src/CashFlow.TransactionsApi"
RUN dotnet build "CashFlow.TransactionsApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CashFlow.TransactionsApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.TransactionsApi.dll"]
