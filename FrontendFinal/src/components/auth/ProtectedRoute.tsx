import React from 'react';
import { useAuth } from 'react-oidc-context';
import { Navigate, useLocation } from 'react-router-dom';
import { ShieldAlert } from 'lucide-react';
import { getUserRoles } from '../../lib/auth-utils';

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles?: string[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, allowedRoles }) => {
    const auth = useAuth();
    const location = useLocation();

    if (auth.isLoading) {
        return (
            <div className="min-h-screen bg-[#0f1115] flex flex-col items-center justify-center">
                <div className="relative">
                    <div className="w-16 h-16 border-4 border-blue-500/20 rounded-full animate-spin border-t-blue-500" />
                    <div className="absolute inset-0 bg-blue-500/10 blur-xl rounded-full" />
                </div>
                <p className="mt-6 text-slate-500 font-black text-xs uppercase tracking-[0.3em] animate-pulse">Verificando Identidad Premium</p>
            </div>
        );
    }

    if (!auth.isAuthenticated) {
        // Redirigir al login si no está autenticado
        // En una app real usaríamos auth.signinRedirect()
        return <Navigate to="/" state={{ from: location }} replace />;
    }

    const userRoles = getUserRoles(auth.user);

    // Si se especifican roles permitidos, verificar que el usuario tenga al menos uno
    if (allowedRoles && allowedRoles.length > 0) {
        const hasPermission = allowedRoles.some(role => userRoles.includes(role.toLowerCase()));

        if (!hasPermission) {
            return (
                <div className="min-h-screen bg-[#0f1115] flex items-center justify-center p-6">
                    <div className="max-w-md w-full bg-[#16191f] border border-slate-800 p-8 rounded-3xl text-center shadow-2xl">
                        <div className="w-16 h-16 bg-rose-500/10 text-rose-500 rounded-2xl flex items-center justify-center mx-auto mb-6">
                            <ShieldAlert className="w-10 h-10" />
                        </div>
                        <h1 className="text-2xl font-black text-white mb-2">Acceso Restringido</h1>
                        <p className="text-slate-400 mb-4">
                            No tienes los permisos necesarios para acceder a esta sección.
                        </p>

                        <div className="bg-black/40 border border-slate-800 rounded-xl p-4 mb-4 text-left">
                            <p className="text-[10px] font-black uppercase tracking-widest text-slate-500 mb-2">Debug Info:</p>
                            <div className="space-y-1">
                                <p className="text-xs text-slate-400"><span className="text-blue-500 font-bold">User ID:</span> {auth.user?.profile.sub}</p>
                                <p className="text-xs text-slate-400"><span className="text-blue-500 font-bold">Roles Encontrados:</span> {userRoles.length > 0 ? userRoles.join(', ') : 'Ninguno'}</p>
                                <p className="text-xs text-slate-400"><span className="text-blue-500 font-bold">Roles Requeridos:</span> {allowedRoles?.join(', ')}</p>
                            </div>
                        </div>

                        <div className="bg-black/20 border border-slate-800/50 rounded-xl p-4 mb-8 text-left overflow-hidden">
                            <p className="text-[10px] font-black uppercase tracking-widest text-slate-600 mb-2">Full Profile (Raw Data):</p>
                            <pre className="text-[9px] text-slate-500 overflow-x-auto custom-scrollbar whitespace-pre-wrap">
                                {JSON.stringify(auth.user?.profile, null, 2)}
                            </pre>
                        </div>
                        <button
                            onClick={() => window.location.href = '/'}
                            className="w-full bg-slate-800 hover:bg-slate-700 text-white font-bold py-3 rounded-xl transition-all"
                        >
                            Volver a la Tienda
                        </button>
                    </div>
                </div>
            );
        }
    }

    return <>{children}</>;
};
