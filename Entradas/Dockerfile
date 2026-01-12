# Dockerfile multi-stage para Entradas.API

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias
COPY ["Entradas.API/Entradas.API.csproj", "Entradas.API/"]
COPY ["Entradas.Aplicacion/Entradas.Aplicacion.csproj", "Entradas.Aplicacion/"]
COPY ["Entradas.Dominio/Entradas.Dominio.csproj", "Entradas.Dominio/"]
COPY ["Entradas.Infraestructura/Entradas.Infraestructura.csproj", "Entradas.Infraestructura/"]

# Restaurar dependencias
RUN dotnet restore "Entradas.API/Entradas.API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar y publicar la aplicación
WORKDIR "/src/Entradas.API"
RUN dotnet build "Entradas.API.csproj" -c Release -o /app/build
RUN dotnet publish "Entradas.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --gid 1001 --no-create-home appuser

# Instalar herramientas de diagnóstico (opcional para debugging)
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar archivos publicados desde build stage
COPY --from=build /app/publish .

# Configurar ownership y permisos
RUN chown -R appuser:appgroup /app
USER appuser

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Configuración de logging
ENV Serilog__MinimumLevel__Default=Information
ENV Serilog__MinimumLevel__Override__Microsoft=Warning
ENV Serilog__MinimumLevel__Override__System=Warning

# Configuración de base de datos (será sobrescrita por docker-compose)
ENV ConnectionStrings__DefaultConnection=""
ENV RabbitMQ__Host=""
ENV RabbitMQ__Username=""
ENV RabbitMQ__Password=""

# Configuración de servicios externos
ENV ServiciosExternos__EventosApi__BaseUrl=""
ENV ServiciosExternos__AsientosApi__BaseUrl=""
ENV ServiciosExternos__Timeout=30

# Configuración de health checks
ENV HealthChecks__Enabled=true

# Exponer puerto
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "Entradas.API.dll"]