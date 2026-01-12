import { useAuth } from 'react-oidc-context';

export const LoginButton = () => {
    const auth = useAuth();

    if (auth.isLoading) {
        return <div className="text-sm text-slate-400">Cargando sesión...</div>;
    }

    if (auth.error) {
        return <div className="text-sm text-red-500">Error: {auth.error.message}</div>;
    }

    if (auth.isAuthenticated) {
        return (
            <div className="flex items-center gap-4">
                <span className="text-sm font-medium text-slate-700">
                    Hola, <span className="text-blue-600">{(auth.user?.profile as any)?.preferred_username}</span>
                </span>
                <button
                    onClick={() => auth.signoutRedirect()}
                    className="px-4 py-2 text-sm font-semibold text-white bg-slate-800 rounded-lg hover:bg-slate-900 transition-colors shadow-sm"
                >
                    Salir
                </button>
            </div>
        );
    }

    return (
        <button
            onClick={() => auth.signinRedirect()}
            className="px-6 py-2 text-sm font-bold text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-all shadow-md shadow-blue-200"
        >
            Iniciar Sesión
        </button>
    );
};
