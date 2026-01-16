import { useState, useEffect } from 'react';
import { X, Sparkles, Calendar, MapPin, ArrowRight, TrendingUp } from 'lucide-react';
import { recomendacionesService, EventoRecomendado } from '../services/recomendaciones.service';
import { useNavigate } from 'react-router-dom';

interface RecomendacionesModalProps {
    onClose: () => void;
}

export default function RecomendacionesModal({ onClose }: RecomendacionesModalProps) {
    const navigate = useNavigate();
    const [recomendaciones, setRecomendaciones] = useState<EventoRecomendado[]>([]);
    const [tendencias, setTendencias] = useState<EventoRecomendado[]>([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'personal' | 'trends'>('personal');

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [misRecs, trends] = await Promise.all([
                recomendacionesService.getMisRecomendaciones().catch(() => ({ recomendaciones: [] })),
                recomendacionesService.getTendencias(5).catch(() => [])
            ]);

            setRecomendaciones(misRecs.recomendaciones || []);
            setTendencias(trends || []);
        } catch (error) {
            console.error('Error cargando recomendaciones', error);
        } finally {
            setLoading(false);
        }
    };

    const handleEventoClick = (id: string) => {
        navigate(`/checkout/${id}`);
        onClose();
    };

    return (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-md flex items-center justify-center z-[100] p-4 animate-in fade-in duration-300">
            <div className="bg-[#12141a] border border-purple-500/20 w-full max-w-4xl max-h-[85vh] rounded-[2rem] overflow-hidden shadow-2xl flex flex-col">

                {/* Header */}
                <div className="p-6 border-b border-white/5 flex items-center justify-between bg-gradient-to-r from-purple-900/20 to-blue-900/20">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-purple-600 rounded-xl flex items-center justify-center shadow-lg shadow-purple-600/20">
                            <Sparkles className="text-white w-5 h-5 animate-pulse" />
                        </div>
                        <div>
                            <h2 className="text-xl font-black text-white uppercase tracking-tight">Descubre tu Próxima Experiencia</h2>
                            <p className="text-[10px] text-purple-300 font-bold uppercase tracking-widest">Powered by Kairo AI</p>
                        </div>
                    </div>
                    <button
                        onClick={onClose}
                        className="p-2 hover:bg-white/10 rounded-full transition-colors text-slate-400 hover:text-white"
                    >
                        <X size={24} />
                    </button>
                </div>

                {/* Tabs */}
                <div className="flex border-b border-white/5">
                    <button
                        onClick={() => setActiveTab('personal')}
                        className={`flex-1 py-4 text-xs font-black uppercase tracking-widest transition-all ${activeTab === 'personal'
                                ? 'bg-purple-500/10 text-purple-400 border-b-2 border-purple-500'
                                : 'text-slate-500 hover:bg-white/5'
                            }`}
                    >
                        <div className="flex items-center justify-center gap-2">
                            <Sparkles size={14} /> Para Ti
                        </div>
                    </button>
                    <button
                        onClick={() => setActiveTab('trends')}
                        className={`flex-1 py-4 text-xs font-black uppercase tracking-widest transition-all ${activeTab === 'trends'
                                ? 'bg-blue-500/10 text-blue-400 border-b-2 border-blue-500'
                                : 'text-slate-500 hover:bg-white/5'
                            }`}
                    >
                        <div className="flex items-center justify-center gap-2">
                            <TrendingUp size={14} /> Tendencias
                        </div>
                    </button>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-y-auto p-6 min-h-[400px]">
                    {loading ? (
                        <div className="flex flex-col items-center justify-center h-full space-y-4">
                            <div className="w-12 h-12 border-4 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
                            <p className="text-slate-500 text-xs font-bold uppercase tracking-widest animate-pulse">Analizando tus gustos...</p>
                        </div>
                    ) : (
                        <div className="grid md:grid-cols-2 gap-4">
                            {(activeTab === 'personal' ? recomendaciones : tendencias).length === 0 ? (
                                <div className="col-span-2 text-center py-20">
                                    <p className="text-slate-500">No encontramos recomendaciones por ahora. ¡Interactúa más con la plataforma!</p>
                                </div>
                            ) : (
                                (activeTab === 'personal' ? recomendaciones : tendencias).map((evento) => (
                                    <div
                                        key={evento.id}
                                        onClick={() => handleEventoClick(evento.id)}
                                        className="group relative bg-white/5 hover:bg-white/10 border border-white/5 hover:border-purple-500/30 rounded-2xl p-4 transition-all cursor-pointer overflow-hidden"
                                    >
                                        <div className="absolute top-0 right-0 p-3 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <div className="bg-purple-600 rounded-full p-1">
                                                <ArrowRight size={14} className="text-white" />
                                            </div>
                                        </div>

                                        <div className="flex gap-4">
                                            <div className="w-20 h-20 bg-slate-800 rounded-xl overflow-hidden flex-shrink-0">
                                                {evento.imagenUrl ? (
                                                    <img src={evento.imagenUrl} alt={evento.titulo} className="w-full h-full object-cover" />
                                                ) : (
                                                    <div className="w-full h-full flex items-center justify-center text-slate-600">
                                                        <Sparkles size={20} />
                                                    </div>
                                                )}
                                            </div>
                                            <div>
                                                <div className="flex items-center gap-2 mb-1">
                                                    <span className="bg-purple-500/20 text-purple-300 text-[9px] font-black uppercase px-2 py-0.5 rounded-full">
                                                        {evento.categoria}
                                                    </span>
                                                    {evento.puntuacion > 0 && (
                                                        <span className="text-[9px] text-green-400 font-bold">
                                                            {Math.round(evento.puntuacion)}% Match
                                                        </span>
                                                    )}
                                                </div>
                                                <h3 className="text-white font-bold leading-tight mb-2 group-hover:text-purple-400 transition-colors">
                                                    {evento.titulo}
                                                </h3>
                                                <div className="space-y-1">
                                                    <div className="flex items-center gap-2 text-slate-400 text-xs">
                                                        <Calendar size={12} />
                                                        <span>{new Date(evento.fechaInicio).toLocaleDateString()}</span>
                                                    </div>
                                                    <div className="flex items-center gap-2 text-slate-400 text-xs">
                                                        <MapPin size={12} />
                                                        <span>{evento.lugar}</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
