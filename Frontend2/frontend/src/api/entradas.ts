import { useQuery } from '@tanstack/react-query';
import { entradasApi } from './clients';
import { Entrada } from '../types/api';

interface ApiResponse<T> {
    success: boolean;
    data: T;
    message?: string;
}

export const useUserTickets = (usuarioId: string) => {
    return useQuery({
        queryKey: ['tickets', 'user', usuarioId],
        queryFn: async () => {
            // The API returns ApiResponse<List<EntradaDto>> or similar
            const { data } = await entradasApi.get<ApiResponse<Entrada[]>>(`/api/entradas/usuario/${usuarioId}`);
            return data.data;
        },
        enabled: !!usuarioId,
    });
};

export const useHasTicketForEvent = (usuarioId: string, eventoId: string) => {
    const { data: tickets, isLoading } = useUserTickets(usuarioId);

    const hasTicket = tickets?.some(t => t.eventoId === eventoId) || false;

    return { hasTicket, isLoading };
};
