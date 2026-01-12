FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5100

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/src/global.json ./global.json
COPY ./backend/src/Services/Asientos/ ./Services/Asientos/
RUN dotnet restore Services/Asientos/Asientos.API/Asientos.API.csproj
RUN dotnet publish Services/Asientos/Asientos.API/Asientos.API.csproj -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5100
ENTRYPOINT ["dotnet", "Asientos.API.dll"]
