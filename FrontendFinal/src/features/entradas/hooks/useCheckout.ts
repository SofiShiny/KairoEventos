import { useState, useEffect, useMemo } from 'react';
import { useAuth } from 'react-oidc-context';
import { useNavigate } from 'react-router-dom';
import { asientosService, Asiento } from '../../asientos/services/asientos.service';
import { eventosService } from '../../eventos/services/eventos.service';
import { Evento } from '../../eventos/types/evento.types';
import { entradasService } from '../services/entradas.service';

export const useCheckout = (eventoId: string | undefined) => {
    const auth = useAuth();
    const navigate = useNavigate();

    const [evento, setEvento] = useState<Evento | null>(null);
    const [asientos, setAsientos] = useState<Asiento[]>([]);
    const [selectedAsientoId, setSelectedAsientoId] = useState<string>('');
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const loadData = async () => {
            if (!eventoId) return;
            try {
                setIsLoading(true);
                const [eventoData, asientosData] = await Promise.all([
                    eventosService.getEventoById(eventoId),
                    asientosService.getAsientosPorEvento(eventoId)
                ]);
                setEvento(eventoData);
                setAsientos(asientosData.filter(a => a.estaDisponible));
            } catch (err: any) {
                console.error(err);
                setError('No se pudo cargar la información necesaria para el checkout.');
            } finally {
                setIsLoading(false);
            }
        };

        loadData();
    }, [eventoId]);

    const selectedAsiento = useMemo(() =>
        asientos.find(a => a.id === selectedAsientoId),
        [asientos, selectedAsientoId]);

    const total = selectedAsiento ? selectedAsiento.precio : 0;

    const handleConfirmarCompra = async () => {
        if (!evento || !selectedAsientoId || !auth.user) {
            setError('Por favor selecciona un asiento y asegúrate de estar autenticado.');
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            await entradasService.crearOrden({
                eventoId: evento.id,
                usuarioId: auth.user.profile.sub || '',
                asientoId: selectedAsientoId,
                cupones: []
            });

            navigate('/perfil');
        } catch (err: any) {
            console.error(err);
            const msg = err.response?.data?.detail || 'Error al procesar la compra.';
            setError(msg);
        } finally {
            setIsSubmitting(false);
        }
    };

    return {
        evento,
        asientos,
        selectedAsientoId,
        setSelectedAsientoId,
        selectedAsiento,
        total,
        isLoading,
        isSubmitting,
        error,
        auth,
        handleConfirmarCompra
    };
};
