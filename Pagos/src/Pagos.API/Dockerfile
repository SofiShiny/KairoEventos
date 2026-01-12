FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Pagos.API/Pagos.API.csproj", "src/Pagos.API/"]
COPY ["src/Pagos.Infraestructura/Pagos.Infraestructura.csproj", "src/Pagos.Infraestructura/"]
COPY ["src/Pagos.Aplicacion/Pagos.Aplicacion.csproj", "src/Pagos.Aplicacion/"]
COPY ["src/Pagos.Dominio/Pagos.Dominio.csproj", "src/Pagos.Dominio/"]
RUN dotnet restore "src/Pagos.API/Pagos.API.csproj"
COPY . .
WORKDIR "/src/src/Pagos.API"
RUN dotnet build "Pagos.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pagos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pagos.API.dll"]
