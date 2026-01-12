import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { eventsApi, seatsApi } from './clients';
import type { Evento, EventoCreateDto } from '../types/api';

// Admin Hooks
export const useAdminEvents = () => {
  return useQuery({
    queryKey: ['events', 'admin'],
    queryFn: async () => {
      const { data } = await eventsApi.get<Evento[]>('/api/Eventos');
      return data;
    },
  });
};

// Organizer Hooks
export const useOrganizerEvents = (organizadorId: string) => {
  return useQuery({
    queryKey: ['events', 'organizer', organizadorId],
    queryFn: async () => {
      const { data } = await eventsApi.get<Evento[]>(`/api/Eventos/organizador/${organizadorId}`);
      return data;
    },
    enabled: !!organizadorId,
  });
};

// Shared Hooks
export const usePublicEvents = () => {
  return useQuery({
    queryKey: ['events', 'public'],
    queryFn: async () => {
      const { data } = await eventsApi.get<Evento[]>('/api/Eventos/publicados');
      return data;
    },
  });
};

export const useEvent = (id: string) => {
  return useQuery({
    queryKey: ['event', id],
    queryFn: async () => {
      const { data } = await eventsApi.get<Evento>(`/api/Eventos/${id}`);
      return data;
    },
    enabled: !!id,
  });
};

// small helper for delay
const delay = (ms: number) => new Promise((res) => setTimeout(res, ms));

export const useCreateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (newEvent: EventoCreateDto) => {
      // 1. Create the Event
      const { data: createdEvent } = await eventsApi.post<Evento>('/api/Eventos', newEvent);

      // 2. Create the Seat Map for the event, with retries
      const maxAttempts = 3;
      let attempt = 0;
      let lastError: any = null;
      let createdMapId: string | null = null;

      while (attempt < maxAttempts) {
        try {
          attempt++;
          // eslint-disable-next-line no-console
          console.debug('[useCreateEvent] creating seat map attempt', attempt, 'for event', createdEvent.id);
          const resp = await seatsApi.post('/api/asientos/mapas', { eventoId: createdEvent.id });
          // eslint-disable-next-line no-console
          console.debug('[useCreateEvent] seat map created', resp.status, resp.data);
          // Expect resp.data contains the created map with an id field (mapaId or id)
          // Try common fields
          createdMapId = resp.data?.mapaId ?? resp.data?.id ?? resp.data?.mapId ?? null;
          lastError = null;
          break;
        } catch (err: any) {
          lastError = err;
          // eslint-disable-next-line no-console
          console.warn('[useCreateEvent] seat map creation failed attempt', attempt, 'status:', err?.response?.status, 'message:', err?.message);
          // exponential backoff before retrying
          if (attempt < maxAttempts) {
            await delay(500 * Math.pow(2, attempt - 1));
          }
        }
      }

      if (lastError) {
        const message = `Evento creado (ID: ${createdEvent.id}), pero falló la creación del mapa de asientos.`;
        const errorToThrow = new Error(message) as any;
        errorToThrow.partialSuccess = true;
        errorToThrow.createdEventId = createdEvent.id;
        errorToThrow.details = lastError?.response?.data ?? lastError?.message;
        throw errorToThrow;
      }

      // If we obtained a createdMapId, persist mapping event->map in localStorage for frontend use
      try {
        if (createdMapId) {
          const key = `map_for_event_${createdEvent.id}`;
          localStorage.setItem(key, createdMapId);
          // eslint-disable-next-line no-console
          console.debug('[useCreateEvent] stored map id mapping', key, '=>', createdMapId);
        }
      } catch (e) {
        // ignore storage errors
      }

      // Success: invalidate events list
      queryClient.invalidateQueries({ queryKey: ['events'] });
      return createdEvent;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

export const useUpdateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, event }: { id: string; event: EventoCreateDto }) => {
      const { data } = await eventsApi.put<Evento>(`/api/Eventos/${id}`, event);
      return data;
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['event', id] });
    },
  });
};

export const useDeleteEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await eventsApi.delete(`/api/Eventos/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

export const usePublishEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      // Assuming patch or put endpoint for publishing
      // The prompt says PATCH (Publicar)
      await eventsApi.patch(`/api/Eventos/${id}/publicar`);
    },
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['event', id] });
    },
  });
};
