import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from 'react-oidc-context';
import { Toaster } from 'react-hot-toast';
import { router } from './router';
import { oidcConfig } from './lib/auth-config';
import { useSignalR } from './hooks/useSignalR';
import { I18nProvider, useT } from './i18n';

// Componente interno que usa el hook de SignalR
function AppContent() {
    const t = useT();
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
            <div className="fixed bottom-4 right-4 z-[9999]">
                <div className={`px-4 py-2 rounded-xl shadow-lg text-sm font-bold flex items-center gap-2 ${isConnected
                    ? 'bg-emerald-500 text-white'
                    : 'bg-rose-500 text-white'
                    }`}>
                    <span className={`w-3 h-3 rounded-full animate-pulse ${isConnected ? 'bg-white' : 'bg-white'}`}></span>
                    {isConnected ? t.system.signalrConnected : t.system.signalrDisconnected}
                    {connectionError && <span className="opacity-70 text-[10px] ml-1">({connectionError})</span>}
                </div>
            </div>
        </>
    );
}

function App() {
    return (
        <I18nProvider>
            <AuthProvider {...oidcConfig}>
                <AppContent />
            </AuthProvider>
        </I18nProvider>
    );
}

export default App;
