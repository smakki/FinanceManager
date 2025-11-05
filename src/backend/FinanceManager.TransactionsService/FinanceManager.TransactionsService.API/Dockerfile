# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082
EXPOSE 8083
EXPOSE 5024

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем все csproj по структуре проекта
COPY FinanceManager.TransactionsService.Abstractions/FinanceManager.TransactionsService.Abstractions.csproj FinanceManager.TransactionsService.Abstractions/
COPY FinanceManager.TransactionsService.API/FinanceManager.TransactionsService.API.csproj FinanceManager.TransactionsService.API/
COPY FinanceManager.TransactionsService.Contracts/FinanceManager.TransactionsService.Contracts.csproj FinanceManager.TransactionsService.Contracts/
COPY FinanceManager.TransactionsService.Domain/FinanceManager.TransactionsService.Domain.csproj FinanceManager.TransactionsService.Domain/
COPY FinanceManager.TransactionsService.EntityFramework/FinanceManager.TransactionsService.EntityFramework.csproj FinanceManager.TransactionsService.EntityFramework/
COPY FinanceManager.TransactionsService.Implementations/FinanceManager.TransactionsService.Implementations.csproj FinanceManager.TransactionsService.Implementations/
COPY FinanceManager.TransactionsService.Repositories/FinanceManager.TransactionsService.Repositories.csproj FinanceManager.TransactionsService.Repositories/

# Восстанавливаем зависимости
RUN dotnet restore FinanceManager.TransactionsService.API/FinanceManager.TransactionsService.API.csproj

# Полный копирование исходников
COPY . .

WORKDIR /src/FinanceManager.TransactionsService.API
RUN dotnet build FinanceManager.TransactionsService.API.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish FinanceManager.TransactionsService.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceManager.TransactionsService.API.dll"]
