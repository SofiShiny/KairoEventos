# Stage 1: Base - Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install curl for health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Stage 2: Build - Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/Gateway.API/Gateway.API.csproj", "Gateway.API/"]
RUN dotnet restore "Gateway.API/Gateway.API.csproj"

# Copy all source files
COPY src/Gateway.API/ Gateway.API/

# Build the application
WORKDIR "/src/Gateway.API"
RUN dotnet build "Gateway.API.csproj" -c Release -o /app/build

# Stage 3: Publish - Publish the application
FROM build AS publish
RUN dotnet publish "Gateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final - Create the final runtime image
FROM base AS final
WORKDIR /app

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Configure health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
