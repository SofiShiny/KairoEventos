
## Requisitos

- .NET 8.0 SDK
- PostgreSQL (para la base de datos)
- RabbitMQ (para la mensajería)
- Docker (opcional, para ejecutar con contenedores)

## Configuración

1. Clonar el repositorio
2. Restaurar los paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Configurar la cadena de conexión a la base de datos en `appsettings.json`
4. Ejecutar las migraciones de Entity Framework si es necesario

## Ejecución

### Ejecución Local

Para ejecutar el proyecto localmente:

```bash
dotnet run --project src/Streaming.API/Streaming.API.csproj
```

La API estará disponible en la URL configurada en `launchSettings.json`.

### Ejecución con Docker

Para construir y ejecutar el proyecto con Docker:

1. Construir la imagen Docker:
   ```bash
   docker build -t streaming-api -f src/Streaming.API/Dockerfile .
   ```

2. Ejecutar el contenedor:
   ```bash
   docker run -p 8080:8080 streaming-api
   ```

La API estará disponible en `http://localhost:8080`.

## Testing

Para ejecutar los tests con cobertura de código:

```bash
dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:coverage-report  

explorer.exe "coverage-report\index.html"
```

