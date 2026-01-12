# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# Relative to context (../Streaming)
COPY ["src/Streaming.API/Streaming.API.csproj", "src/Streaming.API/"]
COPY ["src/Streaming.Dominio/Streaming.Dominio.csproj", "src/Streaming.Dominio/"]
COPY ["src/Streaming.Aplicacion/Streaming.Aplicacion.csproj", "src/Streaming.Aplicacion/"]
COPY ["src/Streaming.Infraestructura/Streaming.Infraestructura.csproj", "src/Streaming.Infraestructura/"]

RUN dotnet restore "src/Streaming.API/Streaming.API.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/src/Streaming.API"
RUN dotnet build "Streaming.API.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "Streaming.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Streaming.API.dll"]
