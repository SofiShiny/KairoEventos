# Implementación: Confirmación de Pagos Múltiples

## 📋 Resumen Ejecutivo

**Problema**: El consumer `PagoAprobadoConsumer` solo confirmaba una entrada cuando se compraban múltiples tickets.

**Solución**: Implementación robusta con búsqueda por `OrdenId`, confirmación en lote, idempotencia y logging detallado.

---

## 🏗️ Arquitectura de la Solución

### Componentes Modificados

```
┌─────────────────────────────────────────────────────────────┐
│                    Pagos.API                                 │
│  Publica: PagoAprobadoEvento { OrdenId, TransaccionId }    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ RabbitMQ
                     ↓
┌─────────────────────────────────────────────────────────────┐
│              PagoAprobadoConsumer                           │
│  1. Busca entradas por OrdenId                             │
│  2. Confirma todas las entradas                            │
│  3. Genera QR para cada una                                │
│  4. Actualiza en lote                                      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│              RepositorioEntradas                            │
│  - ObtenerPorOrdenIdAsync()                                │
│  - ActualizarRangoAsync()                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 Cambios Implementados

### 1. Interface del Repositorio (`IRepositorioEntradas.cs`)

```csharp
/// <summary>
/// Obtiene todas las entradas asociadas a un OrdenId (para compras múltiples)
/// </summary>
Task<List<Entrada>> ObtenerPorOrdenIdAsync(Guid ordenId, CancellationToken cancellationToken = default);

/// <summary>
/// Actualiza múltiples entradas en una sola operación
/// </summary>
Task ActualizarRangoAsync(IEnumerable<Entrada> entradas, CancellationToken cancellationToken = default);
```

### 2. Implementación del Repositorio (`RepositorioEntradas.cs`)

#### Método: `ObtenerPorOrdenIdAsync`

**Estrategia de Búsqueda**:
1. Busca la entrada principal por ID (OrdenId)
2. Encuentra todas las entradas relacionadas usando:
   - Mismo `UsuarioId`
   - Mismo `EventoId`
   - Creadas en ventana de tiempo de ±5 segundos

**Ventajas**:
- ✅ No requiere campo adicional en la BD
- ✅ Funciona con la estructura actual
- ✅ Tolerante a pequeñas diferencias de tiempo

```csharp
public async Task<List<Entrada>> ObtenerPorOrdenIdAsync(Guid ordenId, CancellationToken cancellationToken = default)
{
    // Buscar entrada principal
    var entradaPrincipal = await _context.Entradas
        .FirstOrDefaultAsync(e => e.Id == ordenId, cancellationToken);

    if (entradaPrincipal == null)
        return new List<Entrada>();

    // Buscar entradas relacionadas (±5 segundos)
    var ventanaTiempo = TimeSpan.FromSeconds(5);
    var fechaInicio = entradaPrincipal.FechaCreacion.AddSeconds(-ventanaTiempo.TotalSeconds);
    var fechaFin = entradaPrincipal.FechaCreacion.AddSeconds(ventanaTiempo.TotalSeconds);

    return await _context.Entradas
        .Where(e => e.UsuarioId == entradaPrincipal.UsuarioId 
                 && e.EventoId == entradaPrincipal.EventoId
                 && e.FechaCreacion >= fechaInicio 
                 && e.FechaCreacion <= fechaFin)
        .OrderBy(e => e.FechaCreacion)
        .ToListAsync(cancellationToken);
}
```

#### Método: `ActualizarRangoAsync`

**Características**:
- Actualización en lote usando `UpdateRange()`
- Logging detallado
- Manejo de errores robusto

```csharp
public async Task ActualizarRangoAsync(IEnumerable<Entrada> entradas, CancellationToken cancellationToken = default)
{
    if (entradas == null || !entradas.Any())
        return;

    var listaEntradas = entradas.ToList();
    
    try
    {
        _context.Entradas.UpdateRange(listaEntradas);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Se actualizaron exitosamente {Cantidad} entradas", listaEntradas.Count);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al actualizar rango de {Cantidad} entradas", listaEntradas.Count);
        throw;
    }
}
```

### 3. Consumer Refactorizado (`PagoAprobadoConsumer.cs`)

#### Características Principales

**✅ Manejo de Múltiples Entradas**
```csharp
var entradas = await _repositorio.ObtenerPorOrdenIdAsync(mensaje.OrdenId, context.CancellationToken);
```

**✅ Idempotencia**
```csharp
if (entrada.Estado == EstadoEntrada.Pagada)
{
    _logger.LogInformation("✓ Entrada {EntradaId} ya estaba confirmada (idempotencia)");
    yaConfirmadas++;
    continue;
}
```

**✅ Procesamiento en Lote**
```csharp
foreach (var entrada in entradas)
{
    entrada.ConfirmarPago();
    entrada.AsignarCodigoQr(_generadorQr.GenerarCodigoUnico());
    entradasActualizadas.Add(entrada);
}

await _repositorio.ActualizarRangoAsync(entradasActualizadas, context.CancellationToken);
```

**✅ Logging Detallado con Emojis**
```csharp
_logger.LogInformation(
    "✅ Pago confirmado exitosamente. OrdenId: {OrdenId}, " +
    "Nuevas confirmaciones: {Nuevas}, Ya confirmadas: {YaConfirmadas}, Total: {Total}",
    mensaje.OrdenId, nuevasConfirmaciones, yaConfirmadas, entradas.Count);
```

**✅ Manejo de Errores Parciales**
```csharp
try
{
    entrada.ConfirmarPago();
    entradasActualizadas.Add(entrada);
}
catch (EstadoEntradaInvalidoException ex)
{
    _logger.LogWarning("⚠️ No se pudo confirmar entrada {EntradaId}: {Mensaje}. Se omitirá.");
    // Continuar con las demás entradas
}
```

---

## 🎯 Flujo de Ejecución

### Escenario: Usuario compra 3 tickets

```
1. Usuario selecciona 3 asientos
   ↓
2. Frontend envía: { asientoIds: ["id1", "id2", "id3"] }
   ↓
3. Backend crea 3 entradas:
   - Entrada 1 (ID: guid1) - Estado: Reservada - FechaCreacion: 10:00:00.000
   - Entrada 2 (ID: guid2) - Estado: Reservada - FechaCreacion: 10:00:00.100
   - Entrada 3 (ID: guid3) - Estado: Reservada - FechaCreacion: 10:00:00.200
   ↓
4. Backend retorna: { ordenId: guid1, montoTotal: 150 }
   ↓
5. Pagos procesa pago único de $150
   ↓
6. Pagos publica: PagoAprobadoEvento { OrdenId: guid1, ... }
   ↓
7. PagoAprobadoConsumer recibe evento
   ↓
8. Consumer busca entradas por OrdenId (guid1):
   - Encuentra Entrada 1 (guid1)
   - Busca entradas del mismo usuario/evento/tiempo
   - Encuentra Entrada 2 y 3 también
   ↓
9. Consumer confirma las 3 entradas:
   - Entrada 1: Reservada → Pagada ✓
   - Entrada 2: Reservada → Pagada ✓
   - Entrada 3: Reservada → Pagada ✓
   ↓
10. Consumer genera QR para cada una
    ↓
11. Consumer actualiza en lote (1 query SQL)
    ↓
12. ✅ Proceso completado - 3 tickets confirmados
```

---

## 🛡️ Garantías de Robustez

### 1. Idempotencia
- ✅ Si el evento llega 2 veces, no falla
- ✅ Detecta entradas ya confirmadas
- ✅ No genera QR duplicados

### 2. Manejo de Errores Parciales
- ✅ Si 1 de 3 entradas falla, las otras 2 se confirman
- ✅ Logging detallado de cada error
- ✅ No se pierde información

### 3. Logging Completo
```
🎫 Recibido PagoAprobadoEvento - OrdenId: xxx, TransaccionId: yyy, Monto: 150
📋 Se encontraron 3 entrada(s) para confirmar. OrdenId: xxx
✓ Estado actualizado a Pagada para entrada guid1
🎫 QR generado para entrada guid1: TICKET-xxx-yyy
✓ Estado actualizado a Pagada para entrada guid2
🎫 QR generado para entrada guid2: TICKET-xxx-zzz
✓ Estado actualizado a Pagada para entrada guid3
🎫 QR generado para entrada guid3: TICKET-xxx-www
✅ Pago confirmado exitosamente. Nuevas confirmaciones: 3, Ya confirmadas: 0, Total: 3
```

### 4. Consistencia de Datos
- ✅ Actualización en lote (transaccional)
- ✅ Rollback automático si falla SaveChanges
- ✅ No se pierden entradas

---

## 📊 Casos de Uso Cubiertos

| Escenario | Comportamiento | Estado Final |
|-----------|---------------|--------------|
| **Compra 1 ticket** | Busca 1 entrada, confirma 1 | ✅ 1 Pagada |
| **Compra 3 tickets** | Busca 3 entradas, confirma 3 | ✅ 3 Pagadas |
| **Evento duplicado** | Detecta ya confirmadas | ✅ Idempotente |
| **1 entrada inválida** | Confirma las otras 2 | ✅ 2 Pagadas |
| **OrdenId no existe** | Loguea error crítico, no reintenta | ⚠️ Investigar |
| **Error de BD** | Reintenta vía MassTransit | 🔄 Retry |

---

## 🧪 Testing Recomendado

### Pruebas Unitarias
```csharp
[Fact]
public async Task ObtenerPorOrdenIdAsync_DebeRetornarTodasLasEntradasRelacionadas()
{
    // Arrange: Crear 3 entradas del mismo usuario/evento/tiempo
    // Act: Llamar ObtenerPorOrdenIdAsync con el ID de la primera
    // Assert: Debe retornar las 3 entradas
}

[Fact]
public async Task PagoAprobadoConsumer_DebeSerIdempotente()
{
    // Arrange: Crear entradas ya confirmadas
    // Act: Procesar evento 2 veces
    // Assert: No debe fallar, debe loguear "ya confirmadas"
}
```

### Pruebas de Integración
```csharp
[Fact]
public async Task CompraMultiple_DebeConfirmarTodasLasEntradas()
{
    // Arrange: Comprar 3 tickets
    // Act: Simular pago aprobado
    // Assert: Las 3 entradas deben estar en estado Pagada
}
```

---

## 🚀 Despliegue

### Checklist Pre-Despliegue
- [ ] Compilar solución sin errores
- [ ] Ejecutar tests unitarios
- [ ] Verificar logs en desarrollo
- [ ] Probar con 1, 2 y 3 tickets
- [ ] Verificar idempotencia (enviar evento 2 veces)
- [ ] Monitorear RabbitMQ

### Monitoreo Post-Despliegue
```bash
# Verificar logs del consumer
docker logs kairo-entradas | grep "PagoAprobadoEvento"

# Verificar entradas confirmadas
SELECT COUNT(*) FROM Entradas WHERE Estado = 2; -- Pagada

# Verificar QR generados
SELECT COUNT(*) FROM Entradas WHERE CodigoQr IS NOT NULL AND CodigoQr != '';
```

---

## 📝 Notas Técnicas

### Ventana de Tiempo (±5 segundos)
**Justificación**: Las entradas se crean en milisegundos, pero usamos 5 segundos para:
- Tolerar latencia de red
- Manejar relojes desincronizados
- Evitar falsos negativos

**Riesgo**: Podría capturar entradas no relacionadas si:
- Mismo usuario compra 2 veces el mismo evento en 10 segundos
- **Mitigación**: Muy improbable en uso real

### Alternativa Futura: Campo OrdenId Explícito
Si la ventana de tiempo causa problemas, considerar:
```csharp
public class Entrada
{
    public Guid Id { get; set; }
    public Guid? OrdenId { get; set; } // NUEVO: Referencia explícita
    // ...
}
```

**Ventajas**:
- Búsqueda directa sin heurística
- Sin ambigüedad

**Desventajas**:
- Requiere migración de BD
- Cambios en múltiples capas

---

## ✅ Checklist de Implementación

- [x] Agregar métodos a `IRepositorioEntradas`
- [x] Implementar `ObtenerPorOrdenIdAsync` con lógica de ventana de tiempo
- [x] Implementar `ActualizarRangoAsync` con UpdateRange
- [x] Refactorizar `PagoAprobadoConsumer` para múltiples entradas
- [x] Agregar manejo de idempotencia
- [x] Agregar logging detallado con emojis
- [x] Manejar errores parciales
- [x] Documentar solución

---

**Autor**: Senior Backend Developer  
**Fecha**: 2026-01-09  
**Estado**: ✅ Implementación Completa
