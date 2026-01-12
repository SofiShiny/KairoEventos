FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Marketing.API/Marketing.API.csproj", "src/Marketing.API/"]
COPY ["src/Marketing.Dominio/Marketing.Dominio.csproj", "src/Marketing.Dominio/"]
COPY ["src/Marketing.Aplicacion/Marketing.Aplicacion.csproj", "src/Marketing.Aplicacion/"]
COPY ["src/Marketing.Infraestructura/Marketing.Infraestructura.csproj", "src/Marketing.Infraestructura/"]
RUN dotnet restore "src/Marketing.API/Marketing.API.csproj"
COPY . .
WORKDIR "/src/src/Marketing.API"
RUN dotnet build "Marketing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Marketing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Marketing.API.dll"]
