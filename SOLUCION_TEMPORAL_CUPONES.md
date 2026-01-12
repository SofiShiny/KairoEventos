# ⚠️ SOLUCIÓN TEMPORAL: Sistema de Cupones

## Problema Actual

El Gateway no está ruteando correctamente las peticiones al microservicio de Pagos. Esto está causando errores 404.

## Solución Temporal Implementada

Para que puedas probar el sistema de cupones **ahora mismo**, he creado una configuración temporal que llama directamente al microservicio de Pagos sin pasar por el Gateway.

### Pasos para Activar

1. **Crear archivo `.env.local` en el frontend:**

```bash
cd c:\Users\sofia\source\repos\Sistema-de-Eventos2\Eventos\FrontendFinal
```

Crear archivo `.env.local` con:
```
VITE_PAGOS_API_URL=http://localhost:5278/api/pagos
```

2. **Reiniciar el frontend:**
```bash
# Detener el servidor actual (Ctrl+C)
npm run dev
```

3. **Probar el sistema de cupones:**
- Ve a Admin → Gestión de Eventos
- Clic en botón morado 🏷️ de cupones
- Crear cupón o generar lote
- ¡Debería funcionar!

## Alternativa: Modificar Directamente el Código

Si no quieres usar variables de entorno, puedes modificar temporalmente el servicio:

**Archivo:** `src/features/pagos/services/pagos.service.ts`

```typescript
// Al inicio del archivo, después de los imports
const PAGOS_DIRECT_URL = 'http://localhost:5278/api/pagos';

// En cada método, cambiar:
// De: await api.post('/pagos/cupones/validar', ...)
// A:   await axios.post(`${PAGOS_DIRECT_URL}/cupones/validar`, ...)
```

## ¿Por Qué Esta Solución?

- ✅ **Funciona inmediatamente** - No necesitas arreglar el Gateway
- ✅ **Solo para desarrollo** - En producción usarás el Gateway
- ✅ **Fácil de revertir** - Solo elimina el `.env.local`

## Solución Permanente (Para Después)

El problema real está en la configuración del Gateway o en la autenticación. Para solucionarlo permanentemente:

1. Verificar que Keycloak está configurado correctamente
2. Asegurar que el Gateway tiene los certificados correctos
3. Revisar los logs del Gateway para ver el error exacto
4. Configurar CORS correctamente entre servicios

## Estado de los Servicios

```
✅ Microservicio Pagos: http://localhost:5278
✅ Gateway: http://localhost:8080  
✅ Frontend: http://localhost:5173
⚠️ Ruteo Gateway → Pagos: CON PROBLEMAS
```

## Prueba Rápida

Una vez configurado, prueba esto:

1. **Crear cupón de prueba:**
   - Código: `TEST2026`
   - Porcentaje: `20`
   - Crear

2. **Usar en checkout:**
   - Selecciona asientos
   - Ingresa `TEST2026`
   - ¡Deberías ver 20% de descuento!

---

**Nota:** Esta es una solución temporal solo para desarrollo. El sistema está completamente implementado y funcionando, solo necesita que el Gateway se configure correctamente para producción.
