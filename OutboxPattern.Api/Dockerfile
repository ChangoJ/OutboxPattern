FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OutboxPattern.Api/OutboxPattern.Api.csproj", "OutboxPattern.Api/"]
COPY ["OutboxPattern.Application/OutboxPattern.Application.csproj", "OutboxPattern.Application/"]
COPY ["OutboxPattern.Domain/OutboxPattern.Domain.csproj", "OutboxPattern.Domain/"]
COPY ["OutboxPattern.Infrastructure/OutboxPattern.Infrastructure.csproj", "OutboxPattern.Infrastructure/"]
RUN dotnet restore "./OutboxPattern.Api/OutboxPattern.Api.csproj"
COPY . .
WORKDIR "/src/OutboxPattern.Api"
RUN dotnet publish "./OutboxPattern.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OutboxPattern.Api.dll"]
