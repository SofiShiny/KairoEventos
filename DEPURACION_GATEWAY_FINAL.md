# 🔍 Depuración del Gateway - Diagnóstico Final

## Hallazgos de la Depuración

### ✅ Lo que SÍ funciona:
1. **Keycloak**: Corriendo en puerto 8180 y accesible
2. **Gateway**: Corriendo en puerto 8080
3. **Microservicio Pagos**: Corriendo en puerto 5278
4. **Configuración del Gateway**: Correcta (ruta `/api/pagos/{**catch-all}` → `localhost:5278`)
5. **Controlador de Cupones**: Registrado correctamente (visible en Swagger)

### ❌ El Problema Real:
**El microservicio de Pagos está lanzando errores internos (HTTP 500)**

Cuando llamamos directamente a `http://localhost:5278/api/pagos/cupones/validar`, obtenemos un error del servidor, lo que significa:
- El endpoint existe ✅
- El ruteo funciona ✅
- Pero hay un error en la lógica del servicio ❌

## Causa Probable

El servicio de cupones está intentando acceder a la base de datos, pero:

1. **La migración podría no haberse aplicado correctamente**
2. **La tabla `Cupones` podría no existir**
3. **Hay un error en el código del servicio**

## Solución Recomendada

### Opción 1: Verificar y Aplicar Migración Manualmente

```bash
cd c:\Users\sofia\source\repos\Sistema-de-Eventos2\Eventos\Pagos

# Ver migraciones pendientes
dotnet ef migrations list --project src\Pagos.Infraestructura --startup-project src\Pagos.API

# Aplicar migración
dotnet ef database update --project src\Pagos.Infraestructura --startup-project src\Pagos.API --connection "Host=localhost;Port=5432;Database=pagos_db;Username=postgres;Password=postgres"
```

### Opción 2: Verificar la Base de Datos

Si tienes pgAdmin o DBeaver instalado:
1. Conectar a `localhost:5432`
2. Base de datos: `pagos_db`
3. Usuario: `postgres`
4. Password: `postgres`
5. Verificar que existe la tabla `Cupones`

### Opción 3: Implementación Temporal Sin Base de Datos

Modificar el `CuponServicio` para usar una lista en memoria temporalmente:

```csharp
// En CuponServicio.cs
private static List<Cupon> _cuponesEnMemoria = new();

public async Task<ResultadoValidacionCupon> ValidarCuponAsync(...)
{
    var cupon = _cuponesEnMemoria.FirstOrDefault(c => c.Codigo == codigo.ToUpper());
    // ... resto de la lógica
}
```

## Próximos Pasos

1. **Verificar logs del microservicio de Pagos** para ver el error exacto
2. **Confirmar que la migración se aplicó** y la tabla existe
3. **Probar con un cupón hardcodeado** para verificar que la lógica funciona
4. **Una vez resuelto**, el Gateway funcionará automáticamente

## Estado Actual del Sistema

```
Frontend (5173)
    ↓ HTTP
Gateway (8080)
    ↓ Proxy
Microservicio Pagos (5278)
    ↓ EF Core
PostgreSQL (5432)
    ↓
Base de Datos: pagos_db
    ↓
Tabla: Cupones ← ⚠️ AQUÍ ESTÁ EL PROBLEMA
```

## Comando de Diagnóstico Rápido

Para ver el error exacto, ejecuta:

```bash
cd c:\Users\sofia\source\repos\Sistema-de-Eventos2\Eventos\Pagos

# Detener el servicio actual
# Ctrl+C

# Ejecutar con logs detallados
dotnet run --project src\Pagos.API\Pagos.API.csproj --verbosity detailed

# En otra terminal, hacer la petición
curl -X POST http://localhost:5278/api/pagos/cupones/validar `
  -H "Content-Type: application/json" `
  -d '{"codigo":"TEST","eventoId":null,"montoTotal":100}'

# Ver el error en los logs
```

## Conclusión

El Gateway NO es el problema. El problema está en el microservicio de Pagos, probablemente relacionado con:
1. Migración de base de datos no aplicada
2. Tabla Cupones no existe
3. Error en la lógica del servicio

Una vez resuelto esto, todo el flujo funcionará end-to-end.

---

**Recomendación:** Aplica la migración manualmente con el comando de la Opción 1 y luego reinicia el servicio de Pagos.
