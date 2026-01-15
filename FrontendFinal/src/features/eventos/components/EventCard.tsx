import { useNavigate } from 'react-router-dom';
import { Evento } from '../types/evento.types';
import { useT } from '../../../i18n';

interface Props {
    evento: Evento;
}

export const EventCard = ({ evento }: Props) => {
    const navigate = useNavigate();
    const t = useT();
    const currentLang = document.documentElement.lang === 'es' ? 'es-ES' : 'en-US';

    const formattedDate = new Date(evento.fechaInicio).toLocaleDateString(currentLang, {
        day: '2-digit',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    });

    const placeholderImg = 'https://images.unsplash.com/photo-1501281668745-f7f57925c3b4?q=80&w=1000&auto=format&fit=crop';

    return (
        <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden hover:border-blue-500/50 transition-all group flex flex-col h-full">
            <div className="relative h-48 w-full overflow-hidden">
                <img
                    src={evento.imagenUrl || placeholderImg}
                    alt={evento.titulo}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                />
                <div className="absolute top-4 right-4 bg-blue-600 text-white text-xs font-bold px-2 py-1 rounded">
                    {evento.categoria}
                </div>
            </div>

            <div className="p-5 flex flex-col flex-1">
                <h3 className="text-xl font-bold text-white mb-2 group-hover:text-blue-400 transition-colors">
                    {evento.titulo}
                </h3>

                <p className="text-slate-400 text-sm mb-4 line-clamp-2">
                    {evento.descripcion}
                </p>

                <div className="mt-auto space-y-3">
                    <div className="flex items-center text-slate-300 text-sm">
                        <span className="mr-2">📅</span>
                        {formattedDate}
                    </div>

                    <div className="flex items-center text-slate-300 text-sm">
                        <span className="mr-2">📍</span>
                        {evento.lugar}
                    </div>

                    <div className="flex items-center justify-between pt-4 border-t border-slate-800">
                        <button
                            onClick={() => navigate(`/checkout/${evento.id}`)}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-semibold text-sm transition-colors"
                        >
                            {t.home.viewTickets}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};
