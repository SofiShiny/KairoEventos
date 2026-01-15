import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { getUserRoles } from '../../../lib/auth-utils';
import { useEventos } from '../hooks/useEventos';
import { EventCard } from '../components/EventCard';
import { Sparkles } from 'lucide-react';
import { useT } from '../../../i18n';

export const EventosPage = () => {
    const { data, isLoading, error } = useEventos();
    const auth = useAuth();
    const navigate = useNavigate();
    const t = useT();

    useEffect(() => {
        // Redirigir a admin si el usuario tiene los roles adecuados
        if (auth.isAuthenticated && auth.user) {
            const roles = getUserRoles(auth.user);

            if (roles.includes('admin') || roles.includes('organizator') || roles.includes('organizador')) {
                navigate('/admin', { replace: true });
            }
        }
    }, [auth.isAuthenticated, auth.user, navigate]);

    return (
        <div className="bg-black text-white p-8">
            <div className="max-w-7xl mx-auto">
                <header className="mb-16 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-blue-500 fill-blue-500" />
                        <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">{t.home.exclusiveExperiences}</span>
                    </div>
                    <h1 className="text-6xl font-black mb-6 tracking-tighter">
                        {t.home.discoverNextEvent.split(' ').slice(0, 1)} <br />
                        <span className="bg-gradient-to-r from-white via-white to-neutral-600 bg-clip-text text-transparent">
                            {t.home.discoverNextEvent.split(' ').slice(1).join(' ')}
                        </span>
                    </h1>
                    <p className="text-neutral-500 text-xl max-w-2xl font-medium leading-relaxed">
                        {t.home.heroDescription}
                    </p>
                </header>

                {isLoading && (
                    <div className="flex flex-col items-center justify-center py-32">
                        <div className="relative">
                            <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-blue-600"></div>
                            <div className="absolute inset-0 animate-ping rounded-full h-16 w-16 border border-blue-600/20"></div>
                        </div>
                        <p className="text-neutral-500 mt-6 font-bold tracking-widest text-xs uppercase animate-pulse">{t.home.syncingCatalog}</p>
                    </div>
                )}

                {error && (
                    <div className="bg-red-950/20 border border-red-900/50 p-8 rounded-3xl text-center backdrop-blur-md">
                        <div className="w-16 h-16 bg-red-900/20 rounded-2xl flex items-center justify-center mx-auto mb-4">
                            <span className="text-red-500 text-2xl font-black">!</span>
                        </div>
                        <p className="text-red-400 font-black text-xl mb-2">{t.common.error}</p>
                        <p className="text-red-300/60 text-sm max-w-sm mx-auto">{error}</p>
                        <button
                            onClick={() => window.location.reload()}
                            className="mt-6 px-6 py-2 bg-red-500 text-white font-bold rounded-lg hover:bg-red-600 transition"
                        >
                            {t.common.refresh}
                        </button>
                    </div>
                )}

                {!isLoading && !error && data.length === 0 && (
                    <div className="text-center py-32 border-2 border-dashed border-neutral-900 rounded-[3rem]">
                        <p className="text-neutral-700 text-2xl font-black italic tracking-tighter">{t.home.totalSilence}</p>
                        <p className="text-neutral-500 mt-2">{t.home.noActiveEvents}</p>
                    </div>
                )}

                {!isLoading && !error && data.length > 0 && (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
                        {data.map((evento) => (
                            <EventCard key={evento.id} evento={evento} />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};
