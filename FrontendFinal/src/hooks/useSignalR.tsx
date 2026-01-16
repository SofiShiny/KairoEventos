import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from 'react-oidc-context';
import { toast } from 'react-hot-toast';
import React from 'react';

interface Notificacion {
    tipo: string;
    titulo: string;
    mensaje: string;
    transaccionId?: string;
    ordenId?: string;
    monto?: number;
    urlFactura?: string;
    timestamp: string;
}

export const useSignalR = () => {
    const auth = useAuth();
    const [isConnected, setIsConnected] = useState(false);
    const [connectionError, setConnectionError] = useState<string | null>(null);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    useEffect(() => {
        // depuración
        if (!auth.isAuthenticated) {
            console.log('ℹ️ SignalR: Esperando a que el usuario inicie sesión...');
            return;
        }

        if (!auth.user?.access_token) {
            console.warn('⚠️ SignalR: Usuario autenticado pero sin token.');
            return;
        }

        const gatewayUrl = import.meta.env.VITE_GATEWAY_API_URL || import.meta.env.VITE_API_URL || 'http://localhost:8080';
        const hubUrl = gatewayUrl.includes('/api')
            ? `${gatewayUrl.replace('/api', '')}/hub/notificaciones`
            : `${gatewayUrl}/hub/notificaciones`;

        console.log('🔌 SignalR: Intentando conectar a:', hubUrl);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => auth.user!.access_token,
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('RecibirNotificacion', (notificacion: Notificacion) => {
            console.log('📬 SignalR: Notificación recibida!', notificacion);

            if (notificacion.tipo === 'pago_aprobado') {
                toast.success(
                    React.createElement('div', null, [
                        React.createElement('strong', { key: 't' }, notificacion.titulo),
                        React.createElement('p', { key: 'm', className: 'text-sm mt-1' }, notificacion.mensaje)
                    ]),
                    { duration: 8000, icon: '🎉' }
                );
            } else if (notificacion.tipo === 'pago_rechazado') {
                toast.error(
                    React.createElement('div', null, [
                        React.createElement('strong', { key: 't' }, notificacion.titulo),
                        React.createElement('p', { key: 'm', className: 'text-sm mt-1' }, notificacion.mensaje)
                    ]),
                    { duration: 10000, icon: '⚠️' }
                );
            } else if (notificacion.tipo === 'entrada_cancelada') {
                toast.success(
                    React.createElement('div', null, [
                        React.createElement('strong', { key: 't' }, notificacion.titulo),
                        React.createElement('p', { key: 'm', className: 'text-sm mt-1' }, notificacion.mensaje)
                    ]),
                    { duration: 8000, icon: '💰' }
                );
            } else {
                toast(notificacion.mensaje, { icon: '📬' });
            }
        });

        const start = async () => {
            try {
                await connection.start();
                console.log('✅ SignalR: Conectado exitosamente. ID:', connection.connectionId);
                setIsConnected(true);
                setConnectionError(null);
            } catch (err: any) {
                console.error('❌ SignalR: Error al conectar:', err.message);
                setConnectionError(err.message);
                setIsConnected(false);
            }
        };

        start();
        connectionRef.current = connection;

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
            }
        };
    }, [auth.isAuthenticated, auth.user?.access_token]);

    return { isConnected, connectionError, connection: connectionRef.current };
};
