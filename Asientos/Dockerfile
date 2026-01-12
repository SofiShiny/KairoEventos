# Imagen base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5100

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/ backend/
RUN dotnet restore backend/src/Services/Asientos/Asientos.API/Asientos.API.csproj
RUN dotnet publish backend/src/Services/Asientos/Asientos.API/Asientos.API.csproj -c Release -o /app/publish /p:UseAppHost=true


FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5100
ENV POSTGRES_HOST=postgres
ENV POSTGRES_DB=asientosdb
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=postgres
ENV POSTGRES_PORT=5432
ENTRYPOINT ["dotnet", "Asientos.API.dll"]