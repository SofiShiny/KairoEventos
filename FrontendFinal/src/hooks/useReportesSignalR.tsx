import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from 'react-oidc-context';

export interface MetricasDashboard {
    totalVentasDia: number;
    entradasVendidas: number;
    eventosActivos: number;
    usuariosOnline: number;
    timestamp: string;
}

interface UseReportesSignalRProps {
    onMetricasReceived?: (metricas: MetricasDashboard) => void;
    onVentasRecientesReceived?: (ventas: any[]) => void;
}

export const useReportesSignalR = ({ onMetricasReceived, onVentasRecientesReceived }: UseReportesSignalRProps = {}) => {
    const auth = useAuth();
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    useEffect(() => {
        let gatewayUrl = import.meta.env.VITE_GATEWAY_API_URL || import.meta.env.VITE_API_URL || 'http://localhost:8080';
        // Limpiar sufijo /api para obtener la raíz del gateway
        gatewayUrl = gatewayUrl.replace(/\/api\/?$/, '');
        const hubUrl = `${gatewayUrl}/hub/reportes`;
        console.log('🔌 Intentando conectar a SignalR Hub en:', hubUrl);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => auth.user?.access_token || '',
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('ReceiveMetricas', (metricas: MetricasDashboard) => {
            console.log('📊 Dashboard Metricas:', metricas);
            if (onMetricasReceived) {
                onMetricasReceived(metricas);
            }
        });

        connection.on('ReceiveVentasRecientes', (ventas: any[]) => {
            console.log('💰 Ventas Recientes:', ventas);
            if (onVentasRecientesReceived) {
                onVentasRecientesReceived(ventas);
            }
        });

        const start = async () => {
            try {
                await connection.start();
                console.log('✅ Conectado a Reportes Hub');
                setIsConnected(true);

                await connection.invoke("JoinDashboard");
            } catch (err) {
                console.error('❌ Error conectando a Reportes Hub:', err);
            }
        };

        start();
        connectionRef.current = connection;

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
            }
        };
    }, [auth.user?.access_token]); // Re-conectar si cambia el token

    return { isConnected };
};
