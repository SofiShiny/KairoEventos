FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Recomendaciones.API/Recomendaciones.API.csproj", "Recomendaciones.API/"]
COPY ["Recomendaciones.Dominio/Recomendaciones.Dominio.csproj", "Recomendaciones.Dominio/"]
COPY ["Recomendaciones.Aplicacion/Recomendaciones.Aplicacion.csproj", "Recomendaciones.Aplicacion/"]
COPY ["Recomendaciones.Infraestructura/Recomendaciones.Infraestructura.csproj", "Recomendaciones.Infraestructura/"]
RUN dotnet restore "Recomendaciones.API/Recomendaciones.API.csproj"
COPY . .
WORKDIR "/src/Recomendaciones.API"
RUN dotnet build "Recomendaciones.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Recomendaciones.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recomendaciones.API.dll"]
