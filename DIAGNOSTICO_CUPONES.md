# Prueba Rápida del Sistema de Cupones

## Estado Actual

El sistema de cupones está implementado pero hay un problema de ruteo entre el Gateway y el microservicio de Pagos.

## Diagnóstico

**Problema:** Error 404 al llamar a `/api/pagos/cupones/lote`

**Configuración actual:**
- Frontend: llama a `http://localhost:8080/api/pagos/cupones/lote`
- Gateway: debe rutear a `http://localhost:5278/api/pagos/cupones/lote`
- Microservicio Pagos: escucha en `http://localhost:5278` con ruta `api/pagos/cupones`

## Solución Temporal: Llamar Directamente al Microservicio

Mientras se resuelve el problema del Gateway, puedes probar el sistema llamando directamente al microservicio de Pagos.

### Opción 1: Actualizar el Frontend Temporalmente

Cambiar el baseURL de axios para apuntar directamente a Pagos:

```typescript
// En src/lib/axios.ts (temporal)
const API_BASE_URL = 'http://localhost:5278';
```

### Opción 2: Usar Postman/Thunder Client

**Validar Cupón (No requiere auth):**
```http
POST http://localhost:5278/api/pagos/cupones/validar
Content-Type: application/json

{
  "codigo": "TEST2026",
  "eventoId": "00000000-0000-0000-0000-000000000000",
  "montoTotal": 100.00
}
```

**Crear Cupón General (Requiere admin):**
```http
POST http://localhost:5278/api/pagos/cupones/general
Content-Type: application/json
Authorization: Bearer {tu_token}

{
  "codigo": "PROMO2026",
  "porcentajeDescuento": 20,
  "eventoId": null,
  "esGlobal": true,
  "fechaExpiracion": null,
  "limiteUsos": null
}
```

**Generar Lote (Requiere admin):**
```http
POST http://localhost:5278/api/pagos/cupones/lote
Content-Type: application/json
Authorization: Bearer {tu_token}

{
  "cantidad": 10,
  "porcentajeDescuento": 15,
  "eventoId": "guid-del-evento",
  "fechaExpiracion": null
}
```

## Próximos Pasos para Resolver

1. ✅ Verificar que el Gateway está ruteando correctamente
2. ✅ Asegurar que la configuración de CORS permite las peticiones
3. ✅ Verificar que la autenticación está configurada en el microservicio de Pagos
4. ⏳ Probar el flujo completo end-to-end

## Verificación del Gateway

El Gateway debería tener esta configuración en `appsettings.Development.json`:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "pagos-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5278"
          }
        }
      }
    }
  }
}
```

Y la ruta en `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "pagos-route": {
        "ClusterId": "pagos-cluster",
        "Match": {
          "Path": "/api/pagos/{**catch-all}"
        }
      }
    }
  }
}
```

## Estado de los Servicios

- ✅ Microservicio Pagos: Corriendo en puerto 5278
- ✅ Gateway: Corriendo en puerto 8080
- ✅ Frontend: Corriendo en puerto 5173
- ⚠️ Ruteo: Necesita verificación

## Alternativa: Bypass del Gateway

Si el Gateway sigue dando problemas, puedes configurar el frontend para llamar directamente a cada microservicio en desarrollo:

```typescript
// src/features/pagos/services/pagos.service.ts
const PAGOS_API_URL = import.meta.env.VITE_PAGOS_API_URL || 'http://localhost:5278/api/pagos';

// Usar PAGOS_API_URL en lugar de api.post('/pagos/...')
```

Esto es solo para desarrollo. En producción, todo debe pasar por el Gateway.
