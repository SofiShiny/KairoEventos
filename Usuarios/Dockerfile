# Multi-stage build for Usuarios microservice
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restoration (optimized for layer caching)
COPY ["src/Usuarios.API/Usuarios.API.csproj", "Usuarios.API/"]
COPY ["src/Usuarios.Aplicacion/Usuarios.Aplicacion.csproj", "Usuarios.Aplicacion/"]
COPY ["src/Usuarios.Dominio/Usuarios.Dominio.csproj", "Usuarios.Dominio/"]
COPY ["src/Usuarios.Infraestructura/Usuarios.Infraestructura.csproj", "Usuarios.Infraestructura/"]

# Restore dependencies
RUN dotnet restore "Usuarios.API/Usuarios.API.csproj"

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/Usuarios.API"
RUN dotnet build "Usuarios.API.csproj" -c Release -o /app/build

# Stage 3: Publish image
FROM build AS publish
RUN dotnet publish "Usuarios.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Health check configuration
# Checks the /health endpoint every 30 seconds
# Waits 10 seconds before starting checks (allows app to initialize)
# Times out after 5 seconds
# Retries 3 times before marking as unhealthy
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "Usuarios.API.dll"]
