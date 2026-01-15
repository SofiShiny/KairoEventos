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

        let isMounted = true;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:8080/api'}/../hub/asientos`, {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withAutomaticReconnect()
            .build();

        connection.on('AsientoReservado', (asientoId: string, usuarioId: string) => {
            if (isMounted) {
                console.log('SignalR: Asiento Reservado', { asientoId, usuarioId });
                onAsientoReservado(asientoId, usuarioId);
            }
        });

        connection.on('AsientoLiberado', (asientoId: string) => {
            if (isMounted) {
                console.log('SignalR: Asiento Liberado', { asientoId });
                onAsientoLiberado(asientoId);
            }
        });

        const startConnection = async () => {
            try {
                await connection.start();
                if (!isMounted) {
                    await connection.stop();
                    return;
                }
                console.log('SignalR Connected to AsientosHub');
                if (connection.state === signalR.HubConnectionState.Connected) {
                    await connection.invoke('JoinEvento', eventoId);
                    console.log(`Joined Group: Evento_${eventoId}`);
                }
            } catch (err) {
                if (isMounted) {
                    console.error('SignalR Connection Error: ', err);
                }
            }
        };

        startConnection();
        connectionRef.current = connection;

        return () => {
            isMounted = false;
            const conn = connectionRef.current;
            if (conn) {
                if (conn.state === signalR.HubConnectionState.Connected) {
                    conn.invoke('LeaveEvento', eventoId)
                        .finally(() => conn.stop())
                        .catch(err => console.error('Error leaving group', err));
                } else {
                    conn.stop().catch(err => console.error('Error stopping connection', err));
                }
            }
        };
    }, [eventoId, onAsientoReservado, onAsientoLiberado]);

    return connectionRef.current;
};
