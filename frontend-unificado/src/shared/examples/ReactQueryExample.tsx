/**
 * Example component demonstrating React Query usage
 * 
 * This file shows common patterns for using React Query in the application.
 * It's for reference only and should not be imported in production code.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useMutationWithInvalidation } from '@shared/hooks';

// Example types
interface Evento {
  id: string;
  nombre: string;
  fecha: string;
}

interface CreateEventoDto {
  nombre: string;
  fecha: string;
}

// Example service (would be in @modules/eventos/services)
const eventosService = {
  fetchAll: async (): Promise<Evento[]> => {
    const response = await fetch('/api/eventos');
    return response.json();
  },
  
  fetchById: async (id: string): Promise<Evento> => {
    const response = await fetch(`/api/eventos/${id}`);
    return response.json();
  },
  
  create: async (data: CreateEventoDto): Promise<Evento> => {
    const response = await fetch('/api/eventos', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return response.json();
  },
};

/**
 * Example 1: Simple Query
 * Fetches a list of eventos
 */
export function EventosListExample() {
  const { data: eventos, isLoading, error, refetch } = useQuery({
    queryKey: ['eventos'],
    queryFn: eventosService.fetchAll,
  });

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message} <button onClick={() => refetch()}>Retry</button></div>;

  return (
    <div>
      <h2>Eventos</h2>
      <ul>
        {eventos?.map(evento => (
          <li key={evento.id}>{evento.nombre}</li>
        ))}
      </ul>
    </div>
  );
}

/**
 * Example 2: Query with Parameters
 * Fetches a single evento by ID
 */
export function EventoDetailExample({ eventoId }: { eventoId: string }) {
  const { data: evento, isLoading } = useQuery({
    queryKey: ['eventos', eventoId],
    queryFn: () => eventosService.fetchById(eventoId),
    enabled: !!eventoId, // Only fetch if eventoId exists
  });

  if (isLoading) return <div>Loading evento...</div>;
  if (!evento) return <div>Evento not found</div>;

  return (
    <div>
      <h2>{evento.nombre}</h2>
      <p>Fecha: {evento.fecha}</p>
    </div>
  );
}

/**
 * Example 3: Mutation with Automatic Cache Invalidation
 * Creates a new evento and automatically invalidates related queries
 */
export function CreateEventoExample() {
  const createEvento = useMutationWithInvalidation(
    (data: CreateEventoDto) => eventosService.create(data),
    ['eventos'], // Queries to invalidate after success
    {
      onError: (error) => {
        console.error('Error creating evento:', error);
      },
    }
  );

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    createEvento.mutate({
      nombre: formData.get('nombre') as string,
      fecha: formData.get('fecha') as string,
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <input name="nombre" placeholder="Nombre del evento" required />
      <input name="fecha" type="date" required />
      <button type="submit" disabled={createEvento.isPending}>
        {createEvento.isPending ? 'Creando...' : 'Crear Evento'}
      </button>
      {createEvento.isSuccess && <p>✅ Evento creado exitosamente</p>}
      {createEvento.isError && <p>❌ Error al crear evento</p>}
    </form>
  );
}

/**
 * Example 4: Manual Mutation with Custom Invalidation
 * Shows more control over the mutation process
 */
export function UpdateEventoExample({ eventoId }: { eventoId: string }) {
  const queryClient = useQueryClient();

  const updateEvento = useMutation({
    mutationFn: (data: Partial<Evento>) => 
      fetch(`/api/eventos/${eventoId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }).then(res => res.json()),
    
    onSuccess: () => {
      // Invalidate specific queries
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      queryClient.invalidateQueries({ queryKey: ['eventos', eventoId] });
      console.log('Evento updated successfully');
    },
    
    onError: (error) => {
      console.error('Error updating evento:', error);
    },
  });

  const handleUpdate = () => {
    updateEvento.mutate({ nombre: 'Updated Name' });
  };

  return (
    <button onClick={handleUpdate} disabled={updateEvento.isPending}>
      {updateEvento.isPending ? 'Updating...' : 'Update Evento'}
    </button>
  );
}

/**
 * Example 5: Optimistic Update
 * Updates UI immediately and rolls back on error
 */
export function OptimisticUpdateExample({ eventoId }: { eventoId: string }) {
  const queryClient = useQueryClient();

  const updateEvento = useMutation({
    mutationFn: (newName: string) =>
      fetch(`/api/eventos/${eventoId}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nombre: newName }),
      }).then(res => res.json()),
    
    onMutate: async (newName) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['eventos', eventoId] });

      // Snapshot previous value
      const previousEvento = queryClient.getQueryData<Evento>(['eventos', eventoId]);

      // Optimistically update cache
      if (previousEvento) {
        queryClient.setQueryData<Evento>(['eventos', eventoId], {
          ...previousEvento,
          nombre: newName,
        });
      }

      // Return context with snapshot
      return { previousEvento };
    },
    
    onError: (_err, _newName, context) => {
      // Rollback on error
      if (context?.previousEvento) {
        queryClient.setQueryData(['eventos', eventoId], context.previousEvento);
      }
    },
    
    onSettled: () => {
      // Refetch after error or success
      queryClient.invalidateQueries({ queryKey: ['eventos', eventoId] });
    },
  });

  return (
    <button onClick={() => updateEvento.mutate('New Name')}>
      Update with Optimistic UI
    </button>
  );
}
