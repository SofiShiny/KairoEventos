FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files using correct paths relative to context (../Notificaciones)
COPY ["src/Notificaciones.API/Notificaciones.API.csproj", "src/Notificaciones.API/"]
COPY ["src/Notificaciones.Infraestructura/Notificaciones.Infraestructura.csproj", "src/Notificaciones.Infraestructura/"]
COPY ["src/Notificaciones.Aplicacion/Notificaciones.Aplicacion.csproj", "src/Notificaciones.Aplicacion/"]
COPY ["src/Notificaciones.Dominio/Notificaciones.Dominio.csproj", "src/Notificaciones.Dominio/"]

# Restore dependencies
RUN dotnet restore "src/Notificaciones.API/Notificaciones.API.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/src/Notificaciones.API"
RUN dotnet build "Notificaciones.API.csproj" -c Release -o /app/build
RUN dotnet publish "Notificaciones.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# El contenedor necesita acceso SMTP saliente (puertos 587/465)
ENTRYPOINT ["dotnet", "Notificaciones.API.dll"]
