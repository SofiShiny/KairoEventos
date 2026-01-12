import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { seatsApi } from './clients';
import type { Asiento, Categoria, CategoryCreateDto, SeatCreateDto } from '../types/api';

const STORAGE_KEY = (eventoId: string) => `map_for_event_${eventoId}`;
const CATEGORIES_STORAGE_KEY = (mapaId: string) => `categories_for_map_${mapaId}`;

const extractMapIdFromResponse = (data: any): string | null => {
  return data?.mapaId ?? data?.id ?? data?.mapId ?? null;
};

export const createMapForEvent = async (eventoId: string) => {
  const createPath = `/api/asientos/mapas`;
  // eslint-disable-next-line no-console
  console.debug('[createMapForEvent] POST', `${seatsApi.defaults.baseURL?.replace(/\/$/, '') ?? ''}${createPath}`, { eventoId });
  const resp = await seatsApi.post(createPath, { eventoId });
  const createdMapId = extractMapIdFromResponse(resp.data);
  if (!createdMapId) {
    // If response is the full map, and contains asientos, return them directly
    if (Array.isArray(resp.data?.asientos)) {
      return { mapId: null, seats: resp.data.asientos };
    }
    throw new Error('Map created but response did not include map id');
  }

  try {
    localStorage.setItem(STORAGE_KEY(eventoId), createdMapId);
  } catch (e) {
    // ignore storage errors
  }

  // fetch seats by mapId
  const { data } = await seatsApi.get<Asiento[]>(`/api/asientos/mapas/${createdMapId}`);
  return { mapId: createdMapId, seats: data };
};

export const useEventSeats = (eventoId: string) => {
  return useQuery<Asiento[] | null>({
    queryKey: ['seats', eventoId],
    queryFn: async () => {
      if (!eventoId) return null;

      // Prefer stored map id mapping
      let storedMapId: string | null = null;
      try {
        storedMapId = localStorage.getItem(STORAGE_KEY(eventoId));
      } catch (e) {
        storedMapId = null;
      }

      if (!storedMapId) {
        // No map id mapping found. Do not attempt GET by eventId because API expects mapId.
        // Let the UI decide to create the map.
        // eslint-disable-next-line no-console
        console.debug('[useEventSeats] no stored mapId for event', eventoId);
        return null;
      }

      // Fetch by map id
      const path = `/api/asientos/mapas/${storedMapId}`;
      // eslint-disable-next-line no-console
      console.debug('[useEventSeats] GET by mapId', `${seatsApi.defaults.baseURL?.replace(/\/$/, '') ?? ''}${path}`);
      const { data } = await seatsApi.get<Asiento[]>(path);
      return data;
    },
    enabled: !!eventoId,
  });
};

export const useCreateCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (category: CategoryCreateDto) => {
      // Backend expects camelCase field names (same as TypeScript)
      const payload = {
        nombre: category.nombre,
        precioBase: category.precioBase,
        tienePrioridad: category.tienePrioridad,
        mapaId: category.mapaId,
      };
      // eslint-disable-next-line no-console
      console.log('[useCreateCategory] Category input:', category);
      // eslint-disable-next-line no-console
      console.log('[useCreateCategory] Payload to send:', JSON.stringify(payload, null, 2));
      // eslint-disable-next-line no-console
      console.debug('[useCreateCategory] POST', `${seatsApi.defaults.baseURL?.replace(/\/$/, '') ?? ''}/api/asientos/categorias`, payload);
      const { data } = await seatsApi.post<Categoria>('/api/asientos/categorias', payload);
      // eslint-disable-next-line no-console
      console.debug('[useCreateCategory] Response:', data);
      return data;
    },
    onSuccess: (newCategory, variables) => {
      // eslint-disable-next-line no-console
      console.debug('[useCreateCategory] Success, updating cache and localStorage for mapaId:', variables.mapaId);
      
      // Update the cache directly since backend doesn't have GET endpoint
      queryClient.setQueryData<Categoria[]>(
        ['categories', variables.mapaId],
        (old) => {
          const updated = old ? [...old, newCategory] : [newCategory];
          // eslint-disable-next-line no-console
          console.debug('[useCreateCategory] Updated cache:', updated);
          
          // Also save to localStorage for persistence
          try {
            localStorage.setItem(CATEGORIES_STORAGE_KEY(variables.mapaId), JSON.stringify(updated));
          } catch (e) {
            console.warn('[useCreateCategory] Failed to save to localStorage:', e);
          }
          
          return updated;
        }
      );
    },
  });
};

export const useEventCategories = (mapaId: string) => {
  return useQuery<Categoria[]>({
    queryKey: ['categories', mapaId],
    queryFn: async () => {
      // Backend doesn't have GET endpoint for categories yet
      // Try to load from localStorage
      try {
        const stored = localStorage.getItem(CATEGORIES_STORAGE_KEY(mapaId));
        if (stored) {
          const categories = JSON.parse(stored) as Categoria[];
          // eslint-disable-next-line no-console
          console.debug('[useEventCategories] Loaded from localStorage for mapaId:', mapaId, categories);
          return categories;
        }
      } catch (e) {
        console.warn('[useEventCategories] Failed to load from localStorage:', e);
      }
      
      // eslint-disable-next-line no-console
      console.debug('[useEventCategories] No stored categories, returning empty array for mapaId:', mapaId);
      return [];
    },
    enabled: !!mapaId,
    staleTime: Infinity, // Keep cache indefinitely since we manage it manually
    gcTime: Infinity, // Don't garbage collect
  });
};

export const useCreateSeat = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (seat: SeatCreateDto) => {
      // Backend expects 'categoria' (category name) instead of 'categoriaId'
      const payload = {
        mapaId: seat.mapaId,
        fila: seat.fila,
        numero: seat.numero,
        categoria: seat.categoriaNombre, // Use category name instead of ID
        estado: seat.estado ?? 'Disponible',
      };
      // eslint-disable-next-line no-console
      console.log('[useCreateSeat] Seat input:', seat);
      // eslint-disable-next-line no-console
      console.log('[useCreateSeat] Payload to send:', JSON.stringify(payload, null, 2));
      const { data } = await seatsApi.post<Asiento>('/api/asientos', payload);
      return data;
    },
    onSuccess: () => {
      // Don't invalidate - let user manually refresh
    },
  });
};

export const useReserveSeat = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ mapaId, fila, numero }: { mapaId: string; fila: string | number; numero: number }) => {
      const payload = { mapaId, fila, numero };
      console.log('[useReserveSeat] Reserving seat:', payload);
      const { data } = await seatsApi.post('/api/asientos/reservar', payload);
      return data;
    },
    onSuccess: () => {
      // Invalidate seats queries to refresh the list
      queryClient.invalidateQueries({ queryKey: ['seats'] });
    },
  });
};
