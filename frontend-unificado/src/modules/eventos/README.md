# Módulo de Eventos

Este módulo maneja toda la funcionalidad relacionada con eventos en el sistema.

## Estructura

```
eventos/
├── components/       # Componentes UI (Task 11)
├── hooks/           # Custom hooks con React Query
├── pages/           # Páginas del módulo (Task 11)
├── services/        # API calls al Gateway
├── types/           # TypeScript types e interfaces
└── index.ts         # Barrel export
```

## Servicios (eventosService.ts)

### `fetchEventos()`
Obtiene la lista completa de eventos.
- **Endpoint**: `GET /api/eventos`
- **Returns**: `Promise<Evento[]>`

### `fetchEvento(id: string)`
Obtiene un evento específico por ID.
- **Endpoint**: `GET /api/eventos/:id`
- **Returns**: `Promise<Evento>`

### `createEvento(data: CreateEventoDto)`
Crea un nuevo evento (Admin/Organizator only).
- **Endpoint**: `POST /api/eventos`
- **Returns**: `Promise<Evento>`

### `updateEvento(id: string, data: UpdateEventoDto)`
Actualiza un evento existente (Admin/Organizator only).
- **Endpoint**: `PUT /api/eventos/:id`
- **Returns**: `Promise<Evento>`

### `cancelEvento(id: string)`
Cancela un evento (Admin/Organizator only).
- **Endpoint**: `DELETE /api/eventos/:id/cancelar`
- **Returns**: `Promise<void>`

## Hooks

### `useEventos()`
Hook para obtener la lista de eventos con React Query.

```typescript
const { data: eventos, isLoading, error, refetch } = useEventos();
```

**Características**:
- Cache de 5 minutos (staleTime)
- 3 reintentos automáticos en caso de error
- Invalidación automática al crear/actualizar/cancelar eventos

### `useEvento(id: string)`
Hook para obtener un evento específico por ID.

```typescript
const { data: evento, isLoading, error } = useEvento(eventoId);
```

**Características**:
- Solo ejecuta la query si se proporciona un ID válido
- Cache de 5 minutos
- 3 reintentos automáticos

### `useCreateEvento()`
Hook para crear un nuevo evento.

```typescript
const { mutate: createEvento, isPending, error } = useCreateEvento();

const handleSubmit = (data: CreateEventoDto) => {
  createEvento(data, {
    onSuccess: () => {
      toast.success('Evento creado exitosamente');
      navigate('/eventos');
    },
    onError: (error) => {
      toast.error('Error al crear evento');
    }
  });
};
```

**Características**:
- Invalida automáticamente las queries `['eventos']` y `['dashboard']`
- Manejo de errores integrado
- Loading state automático

### `useUpdateEvento()`
Hook para actualizar un evento existente.

```typescript
const { mutate: updateEvento, isPending, error } = useUpdateEvento();

const handleSubmit = (data: UpdateEventoDto) => {
  updateEvento({ id: eventoId, data }, {
    onSuccess: () => {
      toast.success('Evento actualizado exitosamente');
    },
    onError: (error) => {
      toast.error('Error al actualizar evento');
    }
  });
};
```

**Características**:
- Invalida automáticamente las queries `['eventos']`, `['evento', id]` y `['dashboard']`
- Actualiza el cache del evento específico
- Manejo de errores integrado

### `useCancelarEvento()`
Hook para cancelar un evento.

```typescript
const { mutate: cancelarEvento, isPending, error } = useCancelarEvento();

const handleCancel = () => {
  if (confirm('¿Está seguro de cancelar este evento?')) {
    cancelarEvento(eventoId, {
      onSuccess: () => {
        toast.success('Evento cancelado exitosamente');
        navigate('/eventos');
      },
      onError: (error) => {
        toast.error('Error al cancelar evento');
      }
    });
  }
};
```

**Características**:
- Invalida automáticamente las queries `['eventos']`, `['evento', id]` y `['dashboard']`
- Manejo de errores integrado
- Confirmación recomendada antes de ejecutar

## Tipos

### `Evento`
```typescript
interface Evento {
  id: string;
  nombre: string;
  descripcion: string;
  fecha: string; // ISO 8601
  ubicacion: string;
  imagenUrl?: string;
  estado: 'Publicado' | 'Cancelado';
  capacidadTotal: number;
  asientosDisponibles: number;
}
```

### `CreateEventoDto`
```typescript
interface CreateEventoDto {
  nombre: string;
  descripcion: string;
  fecha: string; // ISO 8601
  ubicacion: string;
  imagenUrl?: string;
}
```

### `UpdateEventoDto`
```typescript
interface UpdateEventoDto {
  nombre?: string;
  descripcion?: string;
  fecha?: string; // ISO 8601
  ubicacion?: string;
  imagenUrl?: string;
}
```

### `EventoFilters`
```typescript
interface EventoFilters {
  fecha?: Date;
  ubicacion?: string;
  busqueda?: string;
}
```

## Ejemplo de Uso Completo

```typescript
import { useEventos, useCreateEvento, useCancelarEvento } from '@modules/eventos';

function EventosPage() {
  const { data: eventos, isLoading, error } = useEventos();
  const { mutate: createEvento } = useCreateEvento();
  const { mutate: cancelarEvento } = useCancelarEvento();

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage error={error} />;

  return (
    <div>
      <h1>Eventos</h1>
      {eventos?.map(evento => (
        <EventoCard 
          key={evento.id} 
          evento={evento}
          onCancel={() => cancelarEvento(evento.id)}
        />
      ))}
    </div>
  );
}
```

## Integración con Gateway

Todos los servicios se comunican exclusivamente con el Gateway en `http://localhost:8080` (configurable via `VITE_GATEWAY_URL`).

El Gateway maneja:
- Autenticación (JWT tokens)
- Autorización (roles)
- Rate limiting
- Routing a microservicios

## Manejo de Errores

Los errores HTTP son manejados automáticamente por el axios interceptor:
- **401**: Redirige al login automáticamente
- **403**: Muestra mensaje de permisos insuficientes
- **404**: Muestra mensaje de recurso no encontrado
- **400**: Propaga errores de validación al formulario
- **500/502/503**: Muestra mensaje de error del servidor con retry automático

## Cache y Invalidación

React Query maneja el cache automáticamente:
- **staleTime**: 5 minutos (datos considerados frescos)
- **cacheTime**: 10 minutos (datos en cache)
- **Invalidación**: Automática al crear/actualizar/cancelar eventos

Las queries invalidadas se refrescan automáticamente si están activas en la UI.

## Próximos Pasos (Task 11)

- Implementar componentes UI: `EventosList`, `EventoCard`, `EventoFilters`
- Implementar páginas completas: `EventosPage`, `EventoDetailPage`
- Agregar formularios de creación/edición con validación
- Implementar búsqueda y filtros
- Agregar tests unitarios para componentes

