# Sistema de Cupones - Documentación de Implementación

## 📋 Resumen

Se ha implementado un sistema completo de cupones de descuento para el checkout de entradas, con funcionalidad de administración para crear cupones generales y generar lotes de cupones únicos.

## 🎯 Funcionalidades Implementadas

### 1. **Servicio de Pagos** (`pagos.service.ts`)

#### Interfaces TypeScript
- `ValidarCuponResponse`: Respuesta de validación de cupón
- `CrearCuponGeneralRequest`: Datos para crear cupón general
- `GenerarLoteCuponesRequest`: Datos para generar lote
- `Cupon`: Modelo de cupón

#### Métodos Agregados
```typescript
// Validar cupón y calcular descuento
validarCupon(codigo: string, eventoId: string, montoTotal: number): Promise<ValidarCuponResponse>

// Crear cupón general (código único usado por muchos)
crearCuponGeneral(data: CrearCuponGeneralRequest): Promise<Cupon>

// Generar lote de cupones únicos (códigos aleatorios de un solo uso)
generarLoteCupones(data: GenerarLoteCuponesRequest): Promise<Cupon[]>

// Obtener cupones por evento
getCuponesPorEvento(eventoId: string): Promise<Cupon[]>

// Obtener cupones globales
getCuponesGlobales(): Promise<Cupon[]>
```

### 2. **Componente de Input de Cupón** (`CouponInput.tsx`)

#### Props
```typescript
interface CouponInputProps {
    onCouponApplied: (codigo: string, descuento: number, nuevoTotal: number) => void;
    onCouponRemoved: () => void;
    eventoId: string;
    montoOriginal: number;
    disabled?: boolean;
}
```

#### Características
- ✅ Validación en tiempo real
- ✅ Animación al aplicar/remover cupón
- ✅ Manejo de estados de carga y error
- ✅ Conversión automática a mayúsculas
- ✅ Feedback visual con colores (verde para éxito, rojo para error)
- ✅ Diseño responsivo con Tailwind CSS

### 3. **Panel de Administración** (`AdminCuponesManager.tsx`)

#### Pestañas

**Pestaña 1: Lista de Cupones**
- Tabla con todos los cupones del evento
- Estados visuales: Activo (verde), Usado (azul), Expirado (rojo)
- Botón "Copiar Todos" - copia todos los códigos al portapapeles
- Botón "Exportar CSV" - descarga archivo CSV con todos los cupones
- Información: Código, Descuento, Tipo, Estado, Fecha de Expiración, Usos

**Pestaña 2: Crear Promoción**
- Formulario para cupón general
- Campos:
  - Código del cupón (manual, en mayúsculas)
  - Descuento en dólares
  - Fecha de expiración (opcional)
  - Checkbox: Cupón global (válido para todos los eventos)

**Pestaña 3: Generar Lote**
- Formulario para generar cupones únicos
- Campos:
  - Cantidad (1-1000 cupones)
  - Descuento por cupón
  - Fecha de expiración (opcional)
- Los códigos se generan automáticamente en el backend

### 4. **Integración en Checkout** (`CheckoutPage.tsx`)

#### Estados Agregados
```typescript
const [cuponAplicado, setCuponAplicado] = useState<{
    codigo: string;
    descuento: number;
    nuevoTotal: number;
} | null>(null);
```

#### Cálculo de Totales
```typescript
const subtotal = selectedAsientos.reduce((sum, asiento) => sum + asiento.precio, 0);
const totalPrice = cuponAplicado ? cuponAplicado.nuevoTotal : subtotal;
```

#### Características
- ✅ Input de cupón visible solo cuando hay asientos seleccionados
- ✅ Resumen muestra:
  - Subtotal original
  - Descuento aplicado (en verde con animación)
  - Total a pagar (cambia de color morado a verde con cupón)
- ✅ Mensaje especial si el total es $0 (entrada gratis)
- ✅ Código de cupón se envía al procesar el pago

### 5. **Actualización de PaymentForm** (`PaymentForm.tsx`)

#### Prop Agregada
```typescript
codigoCupon?: string; // Código de cupón opcional
```

#### Integración
- El código de cupón se incluye en el objeto `PagoRequest`
- Se envía al backend junto con los datos de pago
- El backend valida y aplica el descuento

## 🎨 Diseño y UX

### Colores y Animaciones
- **Cupón aplicado**: Fondo verde degradado con animación fade-in
- **Descuento**: Texto verde con icono de etiqueta
- **Total con descuento**: Gradiente verde esmeralda
- **Total sin descuento**: Gradiente morado-rosa
- **Entrada gratis**: Mensaje animado con emoji 🎉

### Feedback al Usuario
- Toast notifications para:
  - Cupón aplicado exitosamente
  - Cupón removido
  - Errores de validación
  - Cupones creados/generados

## 📡 Endpoints del Backend

```
POST /pagos/cupones/validar
Body: { codigo, eventoId, montoTotal }
Response: { descuento, nuevoTotal, mensaje, porcentajeDescuento }

POST /pagos/cupones/general
Body: { codigo, descuento, fechaExpiracion, eventoId, esGlobal }
Response: Cupon

POST /pagos/cupones/lote
Body: { cantidad, descuento, eventoId, fechaExpiracion }
Response: Cupon[]

GET /pagos/cupones/evento/{eventoId}
Response: Cupon[]

GET /pagos/cupones/globales
Response: Cupon[]
```

## 🔒 Validaciones

### Cliente (Frontend)
- Código no vacío
- Formato en mayúsculas
- Evento válido
- Monto mayor a 0

### Servidor (Backend)
- Cupón existe
- No está expirado
- Aplica al evento específico
- No ha sido usado (cupones únicos)
- Descuento no excede el total

## 💡 Casos de Uso

### Usuario Final
1. Selecciona asientos en el checkout
2. Ingresa código de cupón (ej: "PROMO2026")
3. Clic en "Aplicar"
4. Ve el descuento aplicado en verde
5. Total actualizado automáticamente
6. Procede al pago con el precio con descuento

### Administrador
1. Accede al panel de admin del evento
2. Crea cupón general "VERANO2026" con $20 de descuento
3. O genera lote de 100 cupones únicos de $10
4. Exporta los códigos en CSV
5. Envía cupones por email a clientes

## 🚀 Manejo de Casos Especiales

### Descuento del 100% (Entrada Gratis)
- Si `totalPrice === 0`:
  - Se muestra mensaje "🎉 ¡Entrada gratis con tu cupón!"
  - El frontend permite proceder sin tarjeta
  - Se envían datos dummy si el backend lo requiere

### Cupón Inválido
- Mensaje de error específico:
  - "Cupón expirado"
  - "No aplica a este evento"
  - "Cupón ya utilizado"
  - "Cupón inválido"

### Múltiples Asientos
- El descuento se aplica al total de todos los asientos
- Se muestra claramente:
  - Subtotal de N asientos
  - Descuento aplicado
  - Total final

## 📝 Notas Técnicas

### Type Safety
- Todas las interfaces están tipadas con TypeScript
- Props validadas con PropTypes implícitos
- Respuestas del backend tipadas

### Performance
- Validación de cupón es asíncrona
- Estados de carga para evitar múltiples clicks
- Debouncing implícito (validación solo al hacer clic)

### Accesibilidad
- Labels descriptivos
- Placeholders informativos
- Feedback visual y textual
- Disabled states claros

## 🔄 Flujo Completo

```
1. Usuario selecciona asientos → Subtotal calculado
2. Usuario ingresa cupón → Click "Aplicar"
3. Frontend → POST /pagos/cupones/validar
4. Backend valida y retorna descuento
5. Frontend actualiza UI con descuento
6. Usuario → "Proceder al Pago"
7. Frontend → Crea entradas
8. Modal de pago con total con descuento
9. Frontend → POST /pagos (incluye codigoCupon)
10. Backend procesa pago con descuento aplicado
11. Éxito → Entrada confirmada
```

## ✅ Checklist de Implementación

- [x] Servicio de pagos actualizado con métodos de cupones
- [x] Componente CouponInput creado
- [x] Componente AdminCuponesManager creado
- [x] CheckoutPage integrado con cupones
- [x] PaymentForm actualizado para enviar cupón
- [x] Interfaces TypeScript definidas
- [x] Manejo de errores implementado
- [x] Animaciones y feedback visual
- [x] Exportación CSV de cupones
- [x] Validación cliente y servidor
- [x] Documentación completa

## 🎓 Uso para Administradores

### Crear Cupón para Campaña de Email
1. Ir a Admin → Evento → Cupones
2. Pestaña "Crear Promoción"
3. Código: "NEWSLETTER2026"
4. Descuento: $15
5. Fecha expiración: 31/12/2026
6. Guardar

### Generar Cupones para Sorteo
1. Pestaña "Generar Lote"
2. Cantidad: 50
3. Descuento: $25
4. Generar
5. Exportar CSV
6. Enviar por email a ganadores

---

**Desarrollado con ❤️ usando React, TypeScript y Tailwind CSS**
