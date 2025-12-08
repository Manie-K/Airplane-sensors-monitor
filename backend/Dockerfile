# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/AirplaneSensorsMonitor.csproj backend/
RUN dotnet restore backend/AirplaneSensorsMonitor.csproj
COPY backend/ backend/
WORKDIR /src/backend
RUN dotnet publish AirplaneSensorsMonitor.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY blockchain /app/blockchain
ENTRYPOINT ["dotnet", "AirplaneSensorsMonitor.dll"]
