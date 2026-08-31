FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImmatureBackend.Api/ImmatureBackend.Api.csproj", "ImmatureBackend.Api/"]
COPY ["ImmatureBackend.Application/ImmatureBackend.Application.csproj", "ImmatureBackend.Application/"]
COPY ["ImmatureBackend.Domain/ImmatureBackend.Domain.csproj", "ImmatureBackend.Domain/"]
COPY ["ImmatureBackend.Infrastructure/ImmatureBackend.Infrastructure.csproj", "ImmatureBackend.Infrastructure/"]
RUN dotnet restore "ImmatureBackend.Api/ImmatureBackend.Api.csproj"
COPY . .
WORKDIR "/src/ImmatureBackend.Api"
RUN dotnet build "./ImmatureBackend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImmatureBackend.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImmatureBackend.Api.dll"]
