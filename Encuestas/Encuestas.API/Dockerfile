FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Encuestas.API/Encuestas.API.csproj", "Encuestas.API/"]
COPY ["Encuestas.Dominio/Encuestas.Dominio.csproj", "Encuestas.Dominio/"]
COPY ["Encuestas.Aplicacion/Encuestas.Aplicacion.csproj", "Encuestas.Aplicacion/"]
COPY ["Encuestas.Infraestructura/Encuestas.Infraestructura.csproj", "Encuestas.Infraestructura/"]
RUN dotnet restore "Encuestas.API/Encuestas.API.csproj"
COPY . .
WORKDIR "/src/Encuestas.API"
RUN dotnet build "Encuestas.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Encuestas.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Encuestas.API.dll"]
