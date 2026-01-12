FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files using correct paths relative to context (../Eventos)
COPY ["backend/src/Services/Eventos/Eventos.API/Eventos.API.csproj", "backend/src/Services/Eventos/Eventos.API/"]
COPY ["backend/src/Services/Eventos/Eventos.Aplicacion/Eventos.Aplicacion.csproj", "backend/src/Services/Eventos/Eventos.Aplicacion/"]
COPY ["backend/src/Services/Eventos/Eventos.Dominio/Eventos.Dominio.csproj", "backend/src/Services/Eventos/Eventos.Dominio/"]
COPY ["backend/src/Services/Eventos/Eventos.Infraestructura/Eventos.Infraestructura.csproj", "backend/src/Services/Eventos/Eventos.Infraestructura/"]
COPY ["backend/src/Shared/BloquesConstruccion/Dominio/BloquesConstruccion.Dominio.csproj", "backend/src/Shared/BloquesConstruccion/Dominio/"]
COPY ["backend/src/Shared/BloquesConstruccion/Aplicacion/BloquesConstruccion.Aplicacion.csproj", "backend/src/Shared/BloquesConstruccion/Aplicacion/"]

# Restore dependencies
RUN dotnet restore "backend/src/Services/Eventos/Eventos.API/Eventos.API.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/backend/src/Services/Eventos/Eventos.API"
RUN dotnet build "Eventos.API.csproj" -c Release -o /app/build
RUN dotnet publish "Eventos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Eventos.API.dll"]
