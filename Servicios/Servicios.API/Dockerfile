FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Servicios.API/Servicios.API.csproj", "Servicios.API/"]
COPY ["Servicios.Dominio/Servicios.Dominio.csproj", "Servicios.Dominio/"]
COPY ["Servicios.Aplicacion/Servicios.Aplicacion.csproj", "Servicios.Aplicacion/"]
COPY ["Servicios.Infraestructura/Servicios.Infraestructura.csproj", "Servicios.Infraestructura/"]
RUN dotnet restore "Servicios.API/Servicios.API.csproj"
COPY . .
WORKDIR "/src/Servicios.API"
RUN dotnet build "Servicios.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Servicios.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Servicios.API.dll"]
