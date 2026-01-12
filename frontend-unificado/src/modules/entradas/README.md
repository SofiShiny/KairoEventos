# Módulo de Entradas

Este módulo maneja la gestión de entradas (tickets) para eventos, incluyendo la visualización de entradas del usuario, la selección de asientos disponibles, la creación de nuevas entradas y la cancelación de entradas existentes.

## Estructura

```
entradas/
├── components/       # Componentes UI (a implementar en Task 13)
├── hooks/           # Custom hooks con React Query
├── pages/           # Páginas del módulo (a implementar en Task 13)
├── services/        # Servicios de API
├── types/           # Tipos TypeScript
└── index.ts         # Barrel export
```

## Servicios (entradasService.ts)

### `fetchMisEntradas()`
Obtiene todas las entradas del usuario autenticado.

**Endpoint:** `GET /api/entradas/mis-entradas`

**Returns:** `Promise<Entrada[]>`

### `fetchAsientosDisponibles(eventoId: string)`
Obtiene los asientos disponibles para un evento específico.

**Endpoint:** `GET /api/entradas/asientos-disponibles/:eventoId`

**Returns:** `Promise<Asiento[]>`

### `createEntrada(data: CreateEntradaDto)`
Crea una nueva entrada (reserva un asiento).

**Endpoint:** `POST /api/entradas`

**Returns:** `Promise<Entrada>`

### `cancelarEntrada(id: string)`
Cancela una entrada existente.

**Endpoint:** `DELETE /api/entradas/:id`

**Returns:** `Promise<void>`

## Hooks

### `useMisEntradas(filtro?: FiltroEstadoEntrada)`
Hook para obtener las entradas del usuario con filtrado opcional por estado.

**Parámetros:**
- `filtro` (opcional): 'Todas' | 'Reservada' | 'Pagada' | 'Cancelada'

**Returns:**
```typescript
{
  data: Entrada[];
  isLoading: boolean;
  error: Error | null;
  refetch: () => void;
}
```

**Ejemplo:**
```typescript
const { data: entradas, isLoading } = useMisEntradas('Reservada');
```

### `useAsientosDisponibles(eventoId: string)`
Hook para obtener los asientos disponibles de un evento.

**Parámetros:**
- `eventoId`: ID del evento

**Returns:**
```typescript
{
  data: Asiento[];
  isLoading: boolean;
  error: Error | null;
  refetch: () => void;
}
```

**Ejemplo:**
```typescript
const { data: asientos, isLoading } = useAsientosDisponibles('evento-123');
```

### `useCreateEntrada()`
Hook para crear una nueva entrada (mutation).

**Returns:**
```typescript
{
  mutate: (data: CreateEntradaDto) => void;
  isPending: boolean;
  error: Error | null;
}
```

**Ejemplo:**
```typescript
const { mutate: createEntrada, isPending } = useCreateEntrada();

const handleReserve = () => {
  createEntrada({
    eventoId: 'evento-123',
    asientoId: 'asiento-456',
    usuarioId: 'user-789'
  });
};
```

**Comportamiento:**
- Invalida automáticamente las queries relevantes al completar exitosamente
- Muestra toast de éxito/error
- Actualiza la lista de entradas y asientos disponibles

### `useCancelarEntrada()`
Hook para cancelar una entrada (mutation).

**Returns:**
```typescript
{
  mutate: (entradaId: string) => void;
  isPending: boolean;
  error: Error | null;
}
```

**Ejemplo:**
```typescript
const { mutate: cancelarEntrada, isPending } = useCancelarEntrada();

const handleCancel = (entradaId: string) => {
  if (confirm('¿Está seguro de cancelar esta entrada?')) {
    cancelarEntrada(entradaId);
  }
};
```

**Comportamiento:**
- Invalida automáticamente las queries relevantes al completar exitosamente
- Muestra toast de éxito/error
- Actualiza la lista de entradas y asientos disponibles

## Tipos

### `Entrada`
```typescript
interface Entrada {
  id: string;
  eventoId: string;
  eventoNombre: string;
  asientoId: string;
  asientoInfo: string; // e.g., "Fila A - Asiento 12"
  estado: EstadoEntrada;
  precio: number;
  fechaCompra: string; // ISO 8601
  tiempoRestante?: number; // minutos para pagar (solo Reservada)
}
```

### `Asiento`
```typescript
interface Asiento {
  id: string;
  fila: string;
  numero: number;
  estado: EstadoAsiento;
  precio: number;
}
```

### `EstadoEntrada`
```typescript
type EstadoEntrada = 'Reservada' | 'Pagada' | 'Cancelada';
```

### `EstadoAsiento`
```typescript
type EstadoAsiento = 'Disponible' | 'Reservado' | 'Ocupado';
```

### `CreateEntradaDto`
```typescript
interface CreateEntradaDto {
  eventoId: string;
  asientoId: string;
  usuarioId: string;
}
```

## Gestión de Caché

Los hooks utilizan React Query para gestión de caché:

- **Stale Time:**
  - `useMisEntradas`: 2 minutos (datos específicos del usuario)
  - `useAsientosDisponibles`: 1 minuto (disponibilidad cambia frecuentemente)

- **Invalidación Automática:**
  - Al crear una entrada: invalida `mis-entradas`, `asientos-disponibles`, `eventos`
  - Al cancelar una entrada: invalida `mis-entradas`, `asientos-disponibles`, `eventos`

## Integración con Gateway

Todos los servicios se comunican exclusivamente con el Gateway:
- Base URL: configurada en `VITE_GATEWAY_URL`
- Autenticación: token JWT incluido automáticamente en headers
- Manejo de errores: interceptors de axios manejan errores HTTP

## Testing

Los servicios incluyen tests unitarios usando Vitest:

```bash
npm test -- src/modules/entradas/services/entradasService.test.ts
```

## Próximos Pasos (Task 13)

- Implementar componentes UI:
  - `MisEntradasPage`: lista de entradas del usuario
  - `EntradaCard`: tarjeta individual de entrada
  - `ComprarEntradaPage`: página de compra con mapa de asientos
  - `MapaAsientos`: componente de selección de asientos
- Implementar filtros por estado
- Implementar contador de tiempo restante para pagar
- Implementar confirmación de cancelación

## Requisitos Validados

Este módulo valida los siguientes requisitos:
- **8.1**: Compra de entradas con mapa de asientos
- **8.2**: Visualización de asientos disponibles/reservados/ocupados
- **9.1**: Visualización de entradas del usuario
