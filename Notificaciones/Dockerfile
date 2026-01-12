FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y \
    fontconfig \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Notificaciones.API/Notificaciones.API.csproj", "src/Notificaciones.API/"]
COPY ["src/Notificaciones.Aplicacion/Notificaciones.Aplicacion.csproj", "src/Notificaciones.Aplicacion/"]
COPY ["src/Notificaciones.Dominio/Notificaciones.Dominio.csproj", "src/Notificaciones.Dominio/"]
COPY ["src/Notificaciones.Infraestructura/Notificaciones.Infraestructura.csproj", "src/Notificaciones.Infraestructura/"]
RUN dotnet restore "src/Notificaciones.API/Notificaciones.API.csproj"
COPY . .
WORKDIR "/src/src/Notificaciones.API"
RUN dotnet build "Notificaciones.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notificaciones.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notificaciones.API.dll"]
