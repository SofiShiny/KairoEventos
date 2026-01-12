# Documentación Swagger - Microservicio de Reportes

## Configuración de Swagger

El microservicio de Reportes tiene Swagger completamente configurado y listo para usar.

### Acceso a Swagger UI

Una vez que el servicio esté en ejecución, puedes acceder a Swagger UI en:

```
http://localhost:5003/swagger
```

### Características Implementadas

✅ **Swagger UI Interactivo**
- Interfaz web para explorar y probar todos los endpoints
- Documentación generada automáticamente desde el código
- Capacidad de ejecutar requests directamente desde el navegador

✅ **Documentación Completa**
- Título: "Reportes API v1"
- Descripción detallada del propósito del servicio
- Información de contacto del equipo
- Versión del API

✅ **Anotaciones Habilitadas**
- Uso de `Swashbuckle.AspNetCore.Annotations`
- Comentarios XML generados automáticamente
- Esquemas personalizados para mejor legibilidad

✅ **Generación de XML**
- Documentación XML habilitada en el proyecto
- Comentarios de código incluidos en Swagger
- Warnings de documentación suprimidos (1591)

## Configuración en Program.cs

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() 
    { 
        Title = "Reportes API", 
        Version = "v1",
        Description = "API de reportes y analíticas para el sistema de gestión de eventos. " +
                      "Proporciona endpoints para consultar resúmenes de ventas, asistencia a eventos, " +
                      "logs de auditoría y conciliación financiera.",
        Contact = new()
        {
            Name = "Equipo de Desarrollo",
            Email = "dev@eventos.com"
        }
    });
    
    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Configurar esquemas para mejor documentación
    options.EnableAnnotations();
    options.CustomSchemaIds(type => type.FullName);
});
```

## Configuración en .csproj

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
</ItemGroup>
```

## Endpoints Documentados

### 1. Resumen de Ventas
```
GET /api/reportes/resumen-ventas
```
Parámetros:
- `fechaInicio` (opcional): Fecha de inicio del período
- `fechaFin` (opcional): Fecha de fin del período

### 2. Asistencia por Evento
```
GET /api/reportes/asistencia/{eventoId}
```
Parámetros:
- `eventoId` (requerido): ID del evento

### 3. Logs de Auditoría
```
GET /api/reportes/auditoria
```
Parámetros:
- `fechaInicio` (opcional): Fecha de inicio
- `fechaFin` (opcional): Fecha de fin
- `tipoOperacion` (opcional): Tipo de operación a filtrar
- `pagina` (opcional): Número de página (default: 1)
- `tamañoPagina` (opcional): Tamaño de página (default: 50)

### 4. Conciliación Financiera
```
GET /api/reportes/conciliacion-financiera
```
Parámetros:
- `fechaInicio` (opcional): Fecha de inicio del período
- `fechaFin` (opcional): Fecha de fin del período

## Cómo Usar Swagger UI

### 1. Iniciar el Servicio

```bash
# Opción A: Con Docker Compose
cd Reportes
docker-compose up -d

# Opción B: Directamente con .NET
cd Reportes/backend/src/Services/Reportes/Reportes.API
dotnet run
```

### 2. Acceder a Swagger UI

Abre tu navegador y ve a:
```
http://localhost:5003/swagger
```

### 3. Explorar Endpoints

1. Verás una lista de todos los endpoints disponibles
2. Haz clic en cualquier endpoint para expandirlo
3. Verás la documentación completa incluyendo:
   - Descripción del endpoint
   - Parámetros requeridos y opcionales
   - Tipos de datos
   - Códigos de respuesta posibles
   - Ejemplos de respuesta

### 4. Probar Endpoints

1. Haz clic en el botón "Try it out"
2. Ingresa los parámetros necesarios
3. Haz clic en "Execute"
4. Verás la respuesta completa incluyendo:
   - Código de estado HTTP
   - Headers de respuesta
   - Cuerpo de la respuesta en JSON

## Ejemplo de Uso

### Consultar Resumen de Ventas

1. Expande el endpoint `GET /api/reportes/resumen-ventas`
2. Haz clic en "Try it out"
3. (Opcional) Ingresa fechas:
   - fechaInicio: `2025-01-01`
   - fechaFin: `2025-12-31`
4. Haz clic en "Execute"
5. Verás la respuesta JSON con el resumen de ventas

### Consultar Asistencia de un Evento

1. Expande el endpoint `GET /api/reportes/asistencia/{eventoId}`
2. Haz clic en "Try it out"
3. Ingresa un eventoId válido (GUID)
4. Haz clic en "Execute"
5. Verás la respuesta con los datos de asistencia

## Swagger JSON

También puedes acceder al documento Swagger en formato JSON:

```
http://localhost:5003/swagger/v1/swagger.json
```

Este archivo puede ser usado para:
- Generar clientes automáticamente
- Importar en herramientas como Postman
- Integración con otras herramientas de API

## Personalización

### Agregar Comentarios XML a Endpoints

Para mejorar la documentación, agrega comentarios XML a tus controladores:

```csharp
/// <summary>
/// Obtiene el resumen de ventas para un período específico
/// </summary>
/// <param name="fechaInicio">Fecha de inicio del período (opcional)</param>
/// <param name="fechaFin">Fecha de fin del período (opcional)</param>
/// <returns>Resumen de ventas con totales y métricas</returns>
/// <response code="200">Resumen obtenido exitosamente</response>
/// <response code="400">Parámetros inválidos</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet("resumen-ventas")]
[ProducesResponseType(typeof(ResumenVentasDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ResumenVentasDto>> ObtenerResumenVentas(
    [FromQuery] DateTime? fechaInicio,
    [FromQuery] DateTime? fechaFin)
{
    // Implementación
}
```

### Agregar Ejemplos de Respuesta

Usa anotaciones de Swashbuckle para agregar ejemplos:

```csharp
[SwaggerOperation(
    Summary = "Obtiene el resumen de ventas",
    Description = "Retorna un resumen detallado de ventas con totales, cantidad de reservas y promedio por evento",
    OperationId = "ObtenerResumenVentas",
    Tags = new[] { "Reportes" }
)]
[SwaggerResponse(200, "Resumen obtenido exitosamente", typeof(ResumenVentasDto))]
[SwaggerResponse(400, "Parámetros inválidos")]
[SwaggerResponse(500, "Error interno del servidor")]
```

## Troubleshooting

### Swagger UI no carga

1. Verifica que el servicio esté corriendo:
   ```bash
   curl http://localhost:5003/health
   ```

2. Verifica que estés accediendo a la URL correcta:
   ```
   http://localhost:5003/swagger
   ```

3. Revisa los logs del servicio:
   ```bash
   docker-compose logs reportes-api
   ```

### Documentación XML no aparece

1. Verifica que `GenerateDocumentationFile` esté en `true` en el .csproj
2. Recompila el proyecto:
   ```bash
   dotnet build
   ```
3. Verifica que el archivo XML se generó en la carpeta bin

### Endpoints no aparecen

1. Verifica que los controladores tengan el atributo `[ApiController]`
2. Verifica que los métodos tengan atributos HTTP (`[HttpGet]`, `[HttpPost]`, etc.)
3. Verifica que `AddControllers()` y `MapControllers()` estén configurados

## Recursos Adicionales

- [Documentación oficial de Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Especificación OpenAPI](https://swagger.io/specification/)
- [Swagger UI](https://swagger.io/tools/swagger-ui/)

## Conclusión

Swagger está completamente configurado y funcional en el microservicio de Reportes. Proporciona una interfaz interactiva para explorar y probar todos los endpoints de la API, facilitando el desarrollo, testing y documentación del servicio.

Para acceder, simplemente inicia el servicio y navega a `http://localhost:5003/swagger`.
