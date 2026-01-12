# Solución: Compra Múltiple de Tickets

## Problema Identificado
El sistema solo permitía comprar un ticket a la vez, incluso cuando el usuario seleccionaba múltiples asientos. Esto se debía a que:

1. **Frontend**: Solo procesaba el primer asiento del array (`selectedAsientos[0]`)
2. **Backend**: El DTO solo aceptaba un `AsientoId` individual
3. **Flujo**: No había soporte para transacciones consolidadas

## Solución Implementada

### 1. Backend - DTO Actualizado
**Archivo**: `Entradas.API/DTOs/CrearEntradaRequestDto.cs`

```csharp
public class CrearEntradaRequestDto
{
    public Guid EventoId { get; set; }
    public Guid UsuarioId { get; set; }
    
    // NUEVO: Soporte para múltiples asientos
    public List<Guid>? AsientoIds { get; set; }
    
    // Mantiene compatibilidad con compra individual
    public Guid? AsientoId { get; set; }
    
    public List<string>? Cupones { get; set; }
}
```

### 2. Backend - Controller Actualizado
**Archivo**: `Entradas.API/Controllers/EntradasController.cs`

**Características**:
- ✅ Procesa múltiples asientos en una sola solicitud
- ✅ Crea una entrada por cada asiento
- ✅ Calcula el monto total consolidado
- ✅ Mantiene compatibilidad con compra individual
- ✅ Retorna resumen con todas las entradas creadas

**Respuesta para compra múltiple**:
```json
{
  "data": {
    "entradas": [
      { "id": "guid1", "monto": 50.00, ... },
      { "id": "guid2", "monto": 50.00, ... }
    ],
    "montoTotal": 100.00,
    "cantidad": 2,
    "ordenId": "guid1"
  },
  "success": true
}
```

### 3. Frontend - Service Actualizado
**Archivo**: `entradas.service.ts`

```typescript
export interface BuyTicketPayload {
    eventoId: string;
    usuarioId: string;
    asientoIds: string[]; // Ahora acepta array
    cupones?: string[];
    nombreUsuario?: string;
    email?: string;
}
```

### 4. Frontend - CheckoutPage Actualizado
**Archivo**: `CheckoutPage.tsx`

**Cambios en `handleInitiatePayment`**:
```typescript
// Antes: Solo procesaba el primer asiento
const asiento = selectedAsientos[0];

// Ahora: Procesa todos los asientos seleccionados
const asientoIds = selectedAsientos.map(a => a.id);

const response = await entradasService.crearEntrada({
    eventoId: eventoId!,
    usuarioId: auth.user?.profile.sub || '',
    asientoIds: asientoIds, // Array completo
    ...
});
```

## Flujo de Compra Actualizado

### Paso 1: Selección de Asientos
- Usuario selecciona múltiples asientos en el mapa
- Frontend acumula los asientos en `selectedAsientos[]`
- Se calcula el precio total: `totalPrice = sum(asientos.precio)`

### Paso 2: Creación de Entradas
- Frontend envía `asientoIds: ["id1", "id2", "id3"]` al backend
- Backend itera sobre cada asiento y crea una entrada individual
- Cada entrada se crea en estado `Reservada`
- Backend retorna resumen con:
  - Lista de todas las entradas creadas
  - Monto total consolidado
  - `ordenId` (ID de la primera entrada)

### Paso 3: Procesamiento de Pago
- Se usa el `ordenId` para identificar la transacción
- El monto total se envía al servicio de pagos
- Pagos procesa UN solo pago por el monto consolidado

### Paso 4: Confirmación
- Cuando el pago es aprobado, se publica `PagoAprobadoEvento`
- El `PagoAprobadoConsumer` en Entradas escucha el evento
- **IMPORTANTE**: El evento usa `OrdenId` que corresponde al ID de la primera entrada
- Solo la primera entrada se marca como `Pagada` automáticamente

## ⚠️ Consideración Importante

### Limitación Actual del Consumer
El `PagoAprobadoConsumer` actualmente solo actualiza UNA entrada:

```csharp
// En PagoAprobadoConsumer.cs
var entrada = await _repositorio.ObtenerPorIdAsync(mensaje.OrdenId, ...);
entrada.ConfirmarPago(); // Solo confirma una entrada
```

### Solución Recomendada (Próximo Paso)

Hay dos opciones para resolver esto:

#### Opción A: Modificar el Consumer (Recomendado)
Actualizar `PagoAprobadoConsumer` para buscar todas las entradas asociadas al pago:

```csharp
// Buscar todas las entradas del mismo usuario y evento creadas en la misma transacción
var entradas = await _repositorio.ObtenerPorUsuarioYEventoAsync(
    mensaje.UsuarioId, 
    eventoId, 
    fechaCreacion);

foreach (var entrada in entradas)
{
    entrada.ConfirmarPago();
    await _repositorio.GuardarAsync(entrada);
}
```

#### Opción B: Modificar el Evento de Pago
Incluir una lista de `EntradaIds` en el `PagoAprobadoEvento`:

```csharp
public record PagoAprobadoEvento(
    Guid TransaccionId, 
    Guid OrdenId, 
    List<Guid> EntradaIds, // NUEVO
    Guid UsuarioId, 
    decimal Monto, 
    string UrlFactura
);
```

## Estado Actual

✅ **Funcionando**:
- Selección de múltiples asientos en el frontend
- Creación de múltiples entradas en el backend
- Cálculo de monto total consolidado
- Procesamiento de pago único

⚠️ **Pendiente**:
- Confirmación de TODAS las entradas cuando el pago es aprobado
- Actualmente solo se confirma la primera entrada

## Próximos Pasos

1. **Decidir enfoque**: ¿Modificar el Consumer o el Evento?
2. **Implementar la solución elegida**
3. **Probar el flujo completo** con múltiples asientos
4. **Verificar** que todas las entradas se marquen como `Pagada`

## Archivos Modificados

1. ✅ `Entradas.API/DTOs/CrearEntradaRequestDto.cs`
2. ✅ `Entradas.API/Controllers/EntradasController.cs`
3. ✅ `FrontendFinal/src/features/entradas/services/entradas.service.ts`
4. ✅ `FrontendFinal/src/features/entradas/pages/CheckoutPage.tsx`

## Testing

Para probar la funcionalidad:

1. Seleccionar 2-3 asientos en el mapa
2. Hacer clic en "Proceder al Pago"
3. Completar el formulario de pago
4. Verificar que se creen múltiples entradas en la BD
5. Verificar que el pago procese el monto total
6. **Verificar manualmente** que todas las entradas se marquen como pagadas

---

**Fecha**: 2026-01-09
**Estado**: Implementación parcial completada - Requiere actualización del Consumer
