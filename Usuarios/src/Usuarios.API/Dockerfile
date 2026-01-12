FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files using correct paths relative to context (../Usuarios)
COPY ["src/Usuarios.API/Usuarios.API.csproj", "src/Usuarios.API/"]
COPY ["src/Usuarios.Aplicacion/Usuarios.Aplicacion.csproj", "src/Usuarios.Aplicacion/"]
COPY ["src/Usuarios.Dominio/Usuarios.Dominio.csproj", "src/Usuarios.Dominio/"]
COPY ["src/Usuarios.Infraestructura/Usuarios.Infraestructura.csproj", "src/Usuarios.Infraestructura/"]

# Restore dependencies
RUN dotnet restore "src/Usuarios.API/Usuarios.API.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/src/Usuarios.API"
RUN dotnet build "Usuarios.API.csproj" -c Release -o /app/build
RUN dotnet publish "Usuarios.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Usuarios.API.dll"]