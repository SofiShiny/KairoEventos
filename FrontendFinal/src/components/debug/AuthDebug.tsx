import { useAuth } from 'react-oidc-context';
import { getUserRoles } from '@/lib/auth-utils';

export default function AuthDebug() {
    const auth = useAuth();

    if (!auth.isAuthenticated) {
        return (
            <div className="fixed bottom-4 right-4 bg-red-900/90 text-white p-4 rounded-lg shadow-lg max-w-md">
                <h3 className="font-bold mb-2">🔴 No Autenticado</h3>
                <p className="text-sm">No hay sesión activa</p>
            </div>
        );
    }

    const roles = getUserRoles(auth.user);
    const token = auth.user?.access_token;
    const tokenPreview = token ? `${token.substring(0, 20)}...` : 'No token';

    return (
        <div className="fixed bottom-4 right-4 bg-neutral-900/95 text-white p-4 rounded-lg shadow-lg max-w-md border border-neutral-700">
            <h3 className="font-bold mb-2 text-green-400">✅ Autenticado</h3>
            <div className="text-xs space-y-1">
                <p><strong>Usuario:</strong> {auth.user?.profile?.preferred_username || 'N/A'}</p>
                <p><strong>Email:</strong> {auth.user?.profile?.email || 'N/A'}</p>
                <p><strong>Roles:</strong> {roles.length > 0 ? roles.join(', ') : 'Sin roles'}</p>
                <p><strong>Token:</strong> <code className="bg-neutral-800 px-1 rounded">{tokenPreview}</code></p>
                <p><strong>Es Admin:</strong> {roles.includes('admin') ? '✅ Sí' : '❌ No'}</p>
                <p><strong>Es Organizador:</strong> {roles.includes('organizador') ? '✅ Sí' : '❌ No'}</p>
            </div>
        </div>
    );
}
