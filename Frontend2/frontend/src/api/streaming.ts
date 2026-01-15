import { useQuery } from '@tanstack/react-query';
import { streamingApi } from './clients';
import { Transmision } from '../types/api';

export const useStreaming = (eventoId: string, hasTicket: boolean) => {
    return useQuery({
        queryKey: ['streaming', eventoId],
        queryFn: async () => {
            try {
                const { data } = await streamingApi.get<Transmision>(`/api/streaming/${eventoId}`);
                return data;
            } catch (error: any) {
                if (error.response?.status === 404) {
                    return null;
                }
                throw error;
            }
        },
        enabled: !!eventoId && hasTicket,
        retry: false,
    });
};
