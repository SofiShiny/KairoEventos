/**
 * useMisEntradas Hook
 * Custom hook for fetching user's entradas using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchMisEntradas } from '../services';
import type { Entrada, FiltroEstadoEntrada } from '../types';
import { useMemo } from 'react';
import { useAuth } from '../../../context/AuthContext';

export function useMisEntradas(filtro?: FiltroEstadoEntrada) {
  const { user } = useAuth();
  const userId = user?.profile?.sub;

  const query = useQuery<Entrada[], Error>({
    queryKey: ['mis-entradas', userId],
    queryFn: () => fetchMisEntradas(userId),
    staleTime: 2 * 60 * 1000,
    retry: 3,
    enabled: !!userId, // Solo ejecutar si tenemos el usuario
  });

  // Filter entradas by estado if filtro is provided
  const filteredEntradas = useMemo(() => {
    if (!query.data) return [];
    if (!filtro || filtro === 'Todas') return query.data;

    return query.data.filter((entrada) => entrada.estado === filtro);
  }, [query.data, filtro]);

  return {
    ...query,
    data: filteredEntradas,
  };
}
