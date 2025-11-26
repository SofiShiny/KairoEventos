# Imagen multi-stage para Eventos.API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Puerto expuesto (coincide con UseUrls en Program.cs)
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copiamos todo el backend (simplificado)
COPY ./backend/ ./backend/
RUN dotnet restore backend/src/Services/Eventos/Eventos.API/Eventos.API.csproj
RUN dotnet publish backend/src/Services/Eventos/Eventos.API/Eventos.API.csproj -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENTRYPOINT ["dotnet", "Eventos.API.dll"]
ENV ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=eventsdb;Username=postgres;Password=postgres"
