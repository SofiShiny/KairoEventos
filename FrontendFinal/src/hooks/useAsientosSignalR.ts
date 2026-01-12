import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

interface UseAsientosSignalRProps {
    eventoId: string;
    onAsientoReservado: (asientoId: string, usuarioId: string) => void;
    onAsientoLiberado: (asientoId: string) => void;
}

export const useAsientosSignalR = ({
    eventoId,
    onAsientoReservado,
    onAsientoLiberado,
}: UseAsientosSignalRProps) => {
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    useEffect(() => {
        if (!eventoId) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:8080/api'}/../hub/asientos`, {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withAutomaticReconnect()
            .build();

        connection.on('AsientoReservado', (asientoId: string, usuarioId: string) => {
            console.log('SignalR: Asiento Reservado', { asientoId, usuarioId });
            onAsientoReservado(asientoId, usuarioId);
        });

        connection.on('AsientoLiberado', (asientoId: string) => {
            console.log('SignalR: Asiento Liberado', { asientoId });
            onAsientoLiberado(asientoId);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                console.log('SignalR Connected to AsientosHub');
                await connection.invoke('JoinEvento', eventoId);
                console.log(`Joined Group: Evento_${eventoId}`);
            } catch (err) {
                console.error('SignalR Connection Error: ', err);
            }
        };

        startConnection();
        connectionRef.current = connection;

        return () => {
            if (connectionRef.current) {
                connectionRef.current.invoke('LeaveEvento', eventoId)
                    .then(() => connectionRef.current?.stop())
                    .catch(err => console.error('Error leaving group or stopping connection', err));
            }
        };
    }, [eventoId, onAsientoReservado, onAsientoLiberado]);

    return connectionRef.current;
};
