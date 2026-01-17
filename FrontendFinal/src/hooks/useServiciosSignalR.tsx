import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from 'react-oidc-context';
import { toast } from 'react-hot-toast';
import React from 'react';

interface UseServiciosSignalRProps {
    onNotification?: (notification: any) => void;
}

export const useServiciosSignalR = (props?: UseServiciosSignalRProps) => {
    const auth = useAuth();
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    useEffect(() => {
        const gatewayUrl = import.meta.env.VITE_GATEWAY_API_URL || import.meta.env.VITE_API_URL || 'http://localhost:8080';
        const hubUrl = gatewayUrl.includes('/api')
            ? `${gatewayUrl.replace('/api', '')}/hub/servicios`
            : `${gatewayUrl}/hub/servicios`;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => auth.user?.access_token || '',
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('ReceiveNotification', (notif: any) => {
            console.log('🔔 SignalR Servicios: Notificación recibida!', notif);

            if (props?.onNotification) {
                props.onNotification(notif);
            }

            toast.custom((t) => (
                <div className={`${t.visible ? 'animate-enter' : 'animate-leave'} max-w-md w-full bg-[#1e293b] border border-blue-500/30 shadow-2xl rounded-2xl pointer-events-auto flex ring-1 ring-black ring-opacity-5`}>
                    <div className="flex-1 w-0 p-4">
                        <div className="flex items-start">
                            <div className="flex-shrink-0 pt-0.5">
                                <div className={`h-10 w-10 rounded-full flex items-center justify-center ${notif.tipo === 'success' ? 'bg-green-500/20 text-green-400' : 'bg-amber-500/20 text-amber-400'}`}>
                                    {notif.tipo === 'success' ? '🚀' : '⚠️'}
                                </div>
                            </div>
                            <div className="ml-3 flex-1">
                                <p className="text-sm font-bold text-white">{notif.titulo}</p>
                                <p className="mt-1 text-sm text-slate-400">{notif.mensaje}</p>
                                <p className="mt-2 text-[10px] text-slate-500 italic">Actualizado justo ahora</p>
                            </div>
                        </div>
                    </div>
                </div>
            ), { duration: 6000, position: 'bottom-right' });
        });

        const start = async () => {
            try {
                await connection.start();
                console.log('✅ SignalR Servicios: Conectado exitosamente. ID:', connection.connectionId);
                setIsConnected(true);
            } catch (err: any) {
                console.error('❌ SignalR Servicios: Error al conectar:', err.message);
                setIsConnected(false);
            }
        };

        start();
        connectionRef.current = connection;

        return () => {
            if (connectionRef.current) {
                console.log('🔌 SignalR Servicios: Desconectando...');
                connectionRef.current.stop();
            }
        };
    }, [auth.user?.access_token]);

    return { isConnected };
};
