# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Comunidad.API/Comunidad.API.csproj", "src/Comunidad.API/"]
COPY ["src/Comunidad.Application/Comunidad.Application.csproj", "src/Comunidad.Application/"]
COPY ["src/Comunidad.Infrastructure/Comunidad.Infrastructure.csproj", "src/Comunidad.Infrastructure/"]
COPY ["src/Comunidad.Domain/Comunidad.Domain.csproj", "src/Comunidad.Domain/"]

# Restaurar dependencias
RUN dotnet restore "src/Comunidad.API/Comunidad.API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar y publicar
WORKDIR "/src/src/Comunidad.API"
RUN dotnet build "Comunidad.API.csproj" -c Release -o /app/build
RUN dotnet publish "Comunidad.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Comunidad.API.dll"]
