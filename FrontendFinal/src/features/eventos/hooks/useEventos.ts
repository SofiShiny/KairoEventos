import { useState, useEffect } from 'react';
import { Evento } from '../types/evento.types';
import { eventosService } from '../services/eventos.service';

export const useEventos = () => {
    const [data, setData] = useState<Evento[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchEventos = async () => {
            try {
                setIsLoading(true);
                const eventos = await eventosService.getEventosPublicados();
                setData(eventos);
                setError(null);
            } catch (err: any) {
                setError(err.response?.data?.message || 'Error al cargar los eventos');
            } finally {
                setIsLoading(false);
            }
        };

        fetchEventos();
    }, []);

    return { data, isLoading, error };
};
