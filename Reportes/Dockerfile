FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["backend/src/Services/Reportes/Reportes.API/Reportes.API.csproj", "Services/Reportes/Reportes.API/"]
COPY ["backend/src/Services/Reportes/Reportes.Aplicacion/Reportes.Aplicacion.csproj", "Services/Reportes/Reportes.Aplicacion/"]
COPY ["backend/src/Services/Reportes/Reportes.Dominio/Reportes.Dominio.csproj", "Services/Reportes/Reportes.Dominio/"]
COPY ["backend/src/Services/Reportes/Reportes.Infraestructura/Reportes.Infraestructura.csproj", "Services/Reportes/Reportes.Infraestructura/"]

# Restaurar dependencias
RUN dotnet restore "Services/Reportes/Reportes.API/Reportes.API.csproj"

# Copiar todo el código
COPY backend/src/ .

# Build
WORKDIR "/src/Services/Reportes/Reportes.API"
RUN dotnet build "Reportes.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reportes.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reportes.API.dll"]
