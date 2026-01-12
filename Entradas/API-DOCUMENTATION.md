# Documentación de API - Microservicio Entradas.API

## Información General

- **Base URL**: `http://localhost:5000/api`
- **Versión**: v1
- **Formato**: JSON
- **Autenticación**: Bearer Token (JWT)
- **Content-Type**: `application/json`

## Endpoints

### 1. Crear Entrada

Crea una nueva entrada para un evento específico.

**Endpoint**: `POST /api/entradas`

**Headers**:
```
Content-Type: application/json
Authorization: Bearer {token}
```

**Request Body**:
```json
{
  "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "asientoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monto": 150.00
}
```

**Campos**:
- `eventoId` (required): UUID del evento
- `usuarioId` (required): UUID del usuario
- `asientoId` (optional): UUID del asiento específico (null para entradas generales)
- `monto` (required): Precio de la entrada (decimal, > 0)

**Response 201 - Created**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "asientoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monto": 150.00,
  "codigoQr": "TICKET-A1B2C3D4-5678",
  "estado": "PendientePago",
  "fechaCompra": "2026-01-03T01:00:00Z"
}
```

**Response 400 - Bad Request**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "eventoId": ["El campo EventoId es requerido"],
    "monto": ["El monto debe ser mayor a 0"]
  }
}
```

**Response 404 - Not Found**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Evento No Disponible",
  "status": 404,
  "detail": "El evento especificado no existe o no está disponible"
}
```

**Response 409 - Conflict**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Asiento No Disponible",
  "status": 409,
  "detail": "El asiento seleccionado no está disponible"
}
```

### 2. Obtener Entrada por ID

Obtiene los detalles de una entrada específica.

**Endpoint**: `GET /api/entradas/{id}`

**Parameters**:
- `id` (path): UUID de la entrada

**Headers**:
```
Authorization: Bearer {token}
```

**Response 200 - OK**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "asientoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monto": 150.00,
  "codigoQr": "TICKET-A1B2C3D4-5678",
  "estado": "Pagada",
  "fechaCompra": "2026-01-03T01:00:00Z"
}
```

**Response 404 - Not Found**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Entrada No Encontrada",
  "status": 404,
  "detail": "La entrada con ID especificado no existe"
}
```

### 3. Obtener Entradas por Usuario

Obtiene todas las entradas de un usuario específico.

**Endpoint**: `GET /api/entradas/usuario/{usuarioId}`

**Parameters**:
- `usuarioId` (path): UUID del usuario

**Query Parameters**:
- `page` (optional): Número de página (default: 1)
- `pageSize` (optional): Tamaño de página (default: 10, max: 100)
- `estado` (optional): Filtrar por estado (PendientePago, Pagada, Cancelada, Usada)

**Headers**:
```
Authorization: Bearer {token}
```

**Response 200 - OK**:
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "asientoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "monto": 150.00,
      "codigoQr": "TICKET-A1B2C3D4-5678",
      "estado": "Pagada",
      "fechaCompra": "2026-01-03T01:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

## Estados de Entrada

| Estado | Descripción |
|--------|-------------|
| `PendientePago` | Entrada creada, esperando confirmación de pago |
| `Pagada` | Pago confirmado, entrada válida para uso |
| `Cancelada` | Entrada cancelada, no válida |
| `Usada` | Entrada utilizada para acceder al evento |

## Códigos de Error

### 400 - Bad Request
- Datos de entrada inválidos
- Violación de reglas de validación
- Formato JSON incorrecto

### 401 - Unauthorized
- Token de autenticación faltante o inválido
- Token expirado

### 403 - Forbidden
- Usuario sin permisos para la operación
- Acceso denegado al recurso

### 404 - Not Found
- Entrada no encontrada
- Evento no disponible
- Usuario no encontrado

### 409 - Conflict
- Asiento no disponible
- Estado de entrada inválido para la operación

### 422 - Unprocessable Entity
- Reglas de negocio violadas
- Dependencias no satisfechas

### 500 - Internal Server Error
- Error interno del servidor
- Fallo en servicios externos
- Error de base de datos

### 503 - Service Unavailable
- Servicio temporalmente no disponible
- Mantenimiento programado
- Sobrecarga del sistema

## Health Checks

### Estado General

**Endpoint**: `GET /health`

**Response 200 - Healthy**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "postgresql": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "data": {}
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0234567",
      "data": {}
    },
    "entradas-service": {
      "status": "Healthy",
      "duration": "00:00:00.0012345",
      "data": {}
    }
  }
}
```

### Readiness Probe

**Endpoint**: `GET /health/ready`

Verifica si el servicio está listo para recibir tráfico.

### Liveness Probe

**Endpoint**: `GET /health/live`

Verifica si el servicio está funcionando correctamente.

## Swagger/OpenAPI

### Documentación Interactiva

**URL**: `http://localhost:5000/swagger`

La documentación interactiva de Swagger permite:
- Explorar todos los endpoints disponibles
- Probar requests directamente desde el navegador
- Ver esquemas de datos detallados
- Descargar la especificación OpenAPI

### Especificación OpenAPI

**URL**: `http://localhost:5000/swagger/v1/swagger.json`

Especificación completa en formato JSON para integración con herramientas de desarrollo.

## Ejemplos de Uso

### Crear Entrada con cURL

```bash
curl -X POST "http://localhost:5000/api/entradas" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "asientoId": null,
    "monto": 150.00
  }'
```

### Obtener Entrada con cURL

```bash
curl -X GET "http://localhost:5000/api/entradas/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Obtener Entradas de Usuario con cURL

```bash
curl -X GET "http://localhost:5000/api/entradas/usuario/3fa85f64-5717-4562-b3fc-2c963f66afa6?page=1&pageSize=10&estado=Pagada" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### JavaScript/Fetch

```javascript
// Crear entrada
const crearEntrada = async (entradaData) => {
  const response = await fetch('http://localhost:5000/api/entradas', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(entradaData)
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
};

// Obtener entrada
const obtenerEntrada = async (entradaId) => {
  const response = await fetch(`http://localhost:5000/api/entradas/${entradaId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
};
```

### C# HttpClient

```csharp
public class EntradasApiClient
{
    private readonly HttpClient _httpClient;
    
    public EntradasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
    }
    
    public async Task<EntradaCreadaDto> CrearEntradaAsync(CrearEntradaDto entrada)
    {
        var response = await _httpClient.PostAsJsonAsync("entradas", entrada);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EntradaCreadaDto>();
    }
    
    public async Task<EntradaDto> ObtenerEntradaAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"entradas/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EntradaDto>();
    }
}
```

## Rate Limiting

El API implementa rate limiting para prevenir abuso:

- **Límite por IP**: 100 requests por minuto
- **Límite por usuario**: 50 requests por minuto
- **Headers de respuesta**:
  - `X-RateLimit-Limit`: Límite total
  - `X-RateLimit-Remaining`: Requests restantes
  - `X-RateLimit-Reset`: Timestamp de reset

## Versionado

- **Estrategia**: URL path versioning
- **Versión actual**: v1
- **Formato**: `/api/v{version}/entradas`
- **Compatibilidad**: Mantenida por al menos 6 meses

## Monitoreo

### Métricas Disponibles

- `entradas_created_total`: Total de entradas creadas
- `entradas_by_status`: Entradas agrupadas por estado
- `http_request_duration_seconds`: Duración de requests HTTP
- `external_service_calls_total`: Llamadas a servicios externos

### Logs Estructurados

Todos los logs incluyen:
- `timestamp`: Timestamp ISO 8601
- `level`: Nivel de log (Debug, Info, Warning, Error, Fatal)
- `message`: Mensaje descriptivo
- `correlationId`: ID de correlación para tracing
- `userId`: ID del usuario (cuando aplique)
- `requestId`: ID único del request

### Tracing Distribuido

- **Correlation ID**: Propagado a través de todos los servicios
- **Headers**: `X-Correlation-ID`
- **Logs**: Incluyen correlation ID automáticamente