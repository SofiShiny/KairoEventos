import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Video, AlertCircle, ExternalLink, Globe, Clock, ShieldCheck } from 'lucide-react';
import { streamingService, Transmision } from '../services/streaming.service';

export default function StreamingPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [transmision, setTransmision] = useState<Transmision | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        if (id) {
            loadStreaming();
        }
    }, [id]);

    const loadStreaming = async () => {
        try {
            setLoading(true);
            setError('');

            // Intentar obtener la transmisión existente
            let data = await streamingService.getTransmision(id!);

            // Si no existe, intentar crearla/obtenerla mediante POST
            if (!data) {
                console.log('Transmisión no encontrada, intentando generar una nueva...');
                data = await streamingService.crearObtenerTransmision(id!);
            }

            if (data) {
                setTransmision(data);
            } else {
                setError('No se pudo establecer el canal de streaming. Por favor, verifica que tu entrada sea válida.');
            }
        } catch (err: any) {
            console.error('Error en carga de streaming:', err);
            setError('Error crítico al conectar con el centro de transmisiones.');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="min-h-[80vh] flex flex-col items-center justify-center p-8 bg-black">
                <div className="relative mb-8">
                    <div className="w-24 h-24 border-4 border-blue-600/20 border-t-blue-600 rounded-full animate-spin"></div>
                    <div className="absolute inset-0 flex items-center justify-center">
                        <Video className="w-8 h-8 text-blue-500 animate-pulse" />
                    </div>
                </div>
                <h2 className="text-2xl font-black text-white tracking-widest uppercase italic animate-pulse">Sintonizando...</h2>
                <p className="text-slate-500 mt-2 font-bold uppercase text-[10px] tracking-[0.3em]">Preparando tu acceso premium</p>
            </div>
        );
    }

    if (error || !transmision) {
        return (
            <div className="min-h-[80vh] flex flex-col items-center justify-center p-8 bg-black">
                <div className="bg-[#1a1e26] border border-rose-500/20 p-12 rounded-[3rem] max-w-2xl w-full text-center shadow-2xl shadow-rose-500/5">
                    <div className="w-20 h-20 bg-rose-500/10 rounded-3xl flex items-center justify-center mx-auto mb-8 border border-rose-500/20">
                        <AlertCircle className="w-10 h-10 text-rose-500" />
                    </div>
                    <h1 className="text-4xl font-black text-white mb-4">Misión Interrumpida</h1>
                    <p className="text-lg text-slate-400 mb-8 font-medium leading-relaxed">
                        {error || 'No tienes acceso a esta transmisión o el evento aún no ha comenzado.'}
                    </p>
                    <button
                        onClick={() => navigate('/perfil')}
                        className="px-10 py-4 bg-slate-800 hover:bg-slate-700 text-white font-black rounded-2xl transition-all active:scale-95 uppercase tracking-widest text-xs"
                    >
                        Volver a mis entradas
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white p-4 md:p-12 relative overflow-hidden">
            {/* Efectos de fondo */}
            <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-blue-600/5 blur-[120px] rounded-full pointer-events-none" />
            <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-purple-600/5 blur-[120px] rounded-full pointer-events-none" />

            <div className="max-w-6xl mx-auto">
                <header className="mb-12 flex flex-col md:flex-row md:items-end justify-between gap-6">
                    <div>
                        <div className="flex items-center gap-3 mb-4">
                            <span className="px-3 py-1 bg-rose-600 text-white text-[10px] font-black uppercase tracking-widest rounded-full animate-pulse flex items-center gap-2">
                                <div className="w-1.5 h-1.5 bg-white rounded-full"></div>
                                En Vivo
                            </span>
                            <span className="px-3 py-1 bg-blue-500/10 text-blue-400 text-[10px] font-black uppercase tracking-widest rounded-full border border-blue-500/20">
                                Premium Access
                            </span>
                        </div>
                        <h1 className="text-5xl md:text-6xl font-black tracking-tighter line-clamp-2">
                            TU EXPERIENCIA <br />
                            <span className="bg-gradient-to-r from-blue-400 via-blue-200 to-white bg-clip-text text-transparent">VIRTUAL COMIENZA</span>
                        </h1>
                    </div>

                    <div className="hidden md:flex flex-col items-end">
                        <p className="text-slate-500 text-[10px] font-black uppercase tracking-[0.3em] mb-2 italic">ID de Transmisión</p>
                        <p className="font-mono text-blue-400/50 text-xs">{transmision.id}</p>
                    </div>
                </header>

                <div className="grid lg:grid-cols-3 gap-8">
                    {/* Panel de Visualización (Simulado o Link) */}
                    <div className="lg:col-span-2 space-y-8">
                        <div className="aspect-video bg-[#0f1115] border border-slate-800 rounded-[2.5rem] overflow-hidden relative shadow-2xl group">
                            {/* Placeholder de video premium */}
                            <img
                                src="https://images.unsplash.com/photo-1492684223066-81342ee5ff30?q=80&w=2070&auto=format&fit=crop"
                                className="w-full h-full object-cover opacity-30 blur-sm"
                                alt="Background"
                            />
                            <div className="absolute inset-0 flex flex-col items-center justify-center p-12 text-center bg-black/40 backdrop-blur-[2px]">
                                <div className="w-24 h-24 bg-blue-600 rounded-full flex items-center justify-center mb-8 shadow-2xl shadow-blue-600/40 group-hover:scale-110 transition-transform duration-500">
                                    <Video className="w-10 h-10 text-white" />
                                </div>
                                <h2 className="text-3xl font-black text-white mb-4">SALA DE ESPERA VIP</h2>
                                <p className="text-slate-400 max-w-sm font-medium leading-relaxed mb-10 text-sm">
                                    Todo está listo para la mejor experiencia. Haz clic abajo para unirte a la plataforma de streaming.
                                </p>
                                <a
                                    href={transmision.urlAcceso}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="px-12 py-5 bg-white text-black font-black rounded-[2rem] hover:bg-neutral-200 transition-all flex items-center gap-3 shadow-xl active:scale-95 text-sm"
                                >
                                    ABRIR EN {transmision.plataforma.toUpperCase()}
                                    <ExternalLink className="w-4 h-4" />
                                </a>
                            </div>
                        </div>

                        {/* Detalles adicionales */}
                        <div className="grid md:grid-cols-2 gap-6">
                            <div className="p-8 bg-[#16191f] border border-slate-800 rounded-[2rem]">
                                <Globe className="w-6 h-6 text-blue-500 mb-4" />
                                <h3 className="text-lg font-bold text-white mb-2 uppercase tracking-tight">Plataforma Global</h3>
                                <p className="text-slate-500 text-xs font-bold leading-relaxed uppercase tracking-widest">
                                    Transmitiendo vía {transmision.plataforma}. Compatible con dispositivos móviles, tablets y Smart TVs.
                                </p>
                            </div>
                            <div className="p-8 bg-[#16191f] border border-slate-800 rounded-[2rem]">
                                <ShieldCheck className="w-6 h-6 text-emerald-500 mb-4" />
                                <h3 className="text-lg font-bold text-white mb-2 uppercase tracking-tight">Acceso Encriptado</h3>
                                <p className="text-slate-500 text-xs font-bold leading-relaxed uppercase tracking-widest">
                                    Tu Conexión es segura y privada (SSL 256-bit). Este link es personal e intransferible.
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Lateral: Sidebar de Info */}
                    <div className="space-y-8">
                        <div className="bg-gradient-to-br from-blue-600 to-indigo-900 p-8 rounded-[2.5rem] shadow-2xl shadow-blue-600/20">
                            <h3 className="text-2xl font-black text-white mb-6 italic">RECOMENDACIONES</h3>
                            <ul className="space-y-6">
                                <li className="flex gap-4">
                                    <div className="w-10 h-10 bg-white/10 rounded-xl flex items-center justify-center shrink-0">
                                        <Clock className="w-5 h-5 text-white" />
                                    </div>
                                    <p className="text-xs font-bold text-white/80 leading-relaxed uppercase tracking-widest">Conéctate 10 minutos antes para verificar tu audio.</p>
                                </li>
                                <li className="flex gap-4">
                                    <div className="w-10 h-10 bg-white/10 rounded-xl flex items-center justify-center shrink-0">
                                        <Globe className="w-5 h-5 text-white" />
                                    </div>
                                    <p className="text-xs font-bold text-white/80 leading-relaxed uppercase tracking-widest">Usa una conexión estable de al menos 10Mbps.</p>
                                </li>
                            </ul>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
}
