# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5023

# Образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем все csproj по структуре проекта
COPY FinanceManager.CatalogService.Abstractions/FinanceManager.CatalogService.Abstractions.csproj FinanceManager.CatalogService.Abstractions/
COPY FinanceManager.CatalogService.API/FinanceManager.CatalogService.API.csproj FinanceManager.CatalogService.API/
COPY FinanceManager.CatalogService.Contracts/FinanceManager.CatalogService.Contracts.csproj FinanceManager.CatalogService.Contracts/
COPY FinanceManager.CatalogService.Domain/FinanceManager.CatalogService.Domain.csproj FinanceManager.CatalogService.Domain/
COPY FinanceManager.CatalogService.EntityFramework/FinanceManager.CatalogService.EntityFramework.csproj FinanceManager.CatalogService.EntityFramework/
COPY FinanceManager.CatalogService.Implementations/FinanceManager.CatalogService.Implementations.csproj FinanceManager.CatalogService.Implementations/
COPY FinanceManager.CatalogService.Repositories/FinanceManager.CatalogService.Repositories.csproj FinanceManager.CatalogService.Repositories/

# Восстанавливаем зависимости
RUN dotnet restore FinanceManager.CatalogService.API/FinanceManager.CatalogService.API.csproj

# Полный копирование исходников
COPY . .

WORKDIR /src/FinanceManager.CatalogService.API
RUN dotnet build FinanceManager.CatalogService.API.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish FinanceManager.CatalogService.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceManager.CatalogService.API.dll"]
