import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from 'react-oidc-context';
import { Toaster } from 'react-hot-toast';
import { router } from './router';
import { oidcConfig } from './lib/auth-config';
import { useSignalR } from './hooks/useSignalR';

// Componente interno que usa el hook de SignalR
function AppContent() {
    const { isConnected, connectionError } = useSignalR();

    // Forzar logs de estado
    useEffect(() => {
        console.log('📡 SignalR Current Status:', { isConnected, connectionError });
    }, [isConnected, connectionError]);

    return (
        <>
            <RouterProvider router={router} />
            <Toaster position="top-right" />

            {/* Indicador de conexión SignalR - Siempre visible para diagnóstico */}
            <div className="fixed bottom-4 left-4 z-[9999]">
                <div className={`px-4 py-2 rounded-xl shadow-lg text-sm font-bold flex items-center gap-2 ${isConnected
                    ? 'bg-emerald-500 text-white'
                    : 'bg-rose-500 text-white'
                    }`}>
                    <span className={`w-3 h-3 rounded-full animate-pulse ${isConnected ? 'bg-white' : 'bg-white'}`}></span>
                    {isConnected ? 'SignalR: Conectado' : 'SignalR: Desconectado'}
                    {connectionError && <span className="opacity-70 text-[10px] ml-1">({connectionError})</span>}
                </div>
            </div>
        </>
    );
}

function App() {
    return (
        <AuthProvider {...oidcConfig}>
            <AppContent />
        </AuthProvider>
    );
}

export default App;
