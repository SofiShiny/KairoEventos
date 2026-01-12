import { QueryClient } from '@tanstack/react-query';

/**
 * Configuración del QueryClient de React Query
 * 
 * Opciones configuradas:
 * - staleTime: Tiempo que los datos se consideran frescos (5 minutos)
 * - cacheTime: Tiempo que los datos permanecen en caché (10 minutos)
 * - retry: Número de reintentos en caso de error (2 intentos)
 * - refetchOnWindowFocus: Recargar datos al enfocar la ventana
 * 
 * Performance optimizations:
 * - Prefetching enabled for better UX
 * - Optimized cache times
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Tiempo que los datos se consideran frescos (no se recargan automáticamente)
      staleTime: 5 * 60 * 1000, // 5 minutos
      
      // Tiempo que los datos permanecen en caché después de no usarse
      gcTime: 10 * 60 * 1000, // 10 minutos (antes era cacheTime)
      
      // Número de reintentos en caso de error
      retry: 2,
      
      // Delay entre reintentos (exponencial backoff)
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      
      // Recargar datos cuando la ventana recupera el foco
      refetchOnWindowFocus: true,
      
      // Recargar datos cuando se reconecta la red
      refetchOnReconnect: true,
      
      // No recargar datos al montar el componente si ya están en caché
      refetchOnMount: false,
    },
    mutations: {
      // Reintentar mutaciones fallidas
      retry: 1,
      
      // Delay entre reintentos para mutaciones
      retryDelay: 1000,
    },
  },
});

/**
 * Función para invalidar todas las queries relacionadas con un recurso
 * Útil después de crear, actualizar o eliminar datos
 */
export const invalidateQueries = (queryKeys: string[]) => {
  queryKeys.forEach((key) => {
    queryClient.invalidateQueries({ queryKey: [key] });
  });
};

/**
 * Función para prefetch de datos
 * Útil para cargar datos antes de que el usuario los necesite
 * 
 * @example
 * // Prefetch evento details when hovering over evento card
 * prefetchQuery(['evento', eventoId], () => fetchEvento(eventoId));
 */
export const prefetchQuery = async <TData>(
  queryKey: unknown[],
  queryFn: () => Promise<TData>,
  staleTime?: number
) => {
  await queryClient.prefetchQuery({
    queryKey,
    queryFn,
    staleTime: staleTime ?? 5 * 60 * 1000, // Default 5 minutes
  });
};

/**
 * Función para limpiar toda la caché de React Query
 * Se debe llamar al cerrar sesión
 */
export const clearQueryCache = () => {
  queryClient.clear();
};
