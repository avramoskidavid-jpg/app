# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so `dotnet restore` is cached independently of source changes.
COPY src/EnergyManagement.Core/EnergyManagement.Core.csproj src/EnergyManagement.Core/
COPY src/EnergyManagement.Application/EnergyManagement.Application.csproj src/EnergyManagement.Application/
COPY src/EnergyManagement.Infrastructure/EnergyManagement.Infrastructure.csproj src/EnergyManagement.Infrastructure/
COPY src/EnergyManagement.Api/EnergyManagement.Api.csproj src/EnergyManagement.Api/
RUN dotnet restore src/EnergyManagement.Api/EnergyManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/EnergyManagement.Api/EnergyManagement.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnergyManagement.Api.dll"]
