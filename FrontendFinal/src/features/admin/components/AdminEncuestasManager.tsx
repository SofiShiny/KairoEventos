import { useState, useEffect } from 'react';
import {
    Plus,
    Trash2,
    Save,
    CheckCircle,
    AlertCircle,
    Loader2,
    Star,
    Type,
    BarChart3,
    Clock,
    User,
    ChevronDown,
    ChevronUp,
    MessageSquare,
    Zap
} from 'lucide-react';
import { encuestasService, Encuesta, TipoPregunta } from '../../encuestas/services/encuestas.service';
import { toast } from 'react-hot-toast';

interface AdminEncuestasManagerProps {
    eventoId: string;
}

export default function AdminEncuestasManager({ eventoId }: AdminEncuestasManagerProps) {
    const [encuesta, setEncuesta] = useState<Encuesta | null>(null);
    const [respuestas, setRespuestas] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [showResults, setShowResults] = useState(false);
    const [expandedResp, setExpandedResp] = useState<string | null>(null);

    // Form states for new survey
    const [titulo, setTitulo] = useState('Encuesta de Satisfacción');
    const [preguntas, setPreguntas] = useState<{ enunciado: string, tipo: TipoPregunta }[]>([
        { enunciado: '¿Qué te pareció el evento en general?', tipo: TipoPregunta.Estrellas },
        { enunciado: '¿Qué mejorarías para la próxima edición?', tipo: TipoPregunta.Texto }
    ]);

    useEffect(() => {
        cargarEncuesta();
    }, [eventoId]);

    const cargarEncuesta = async () => {
        try {
            setLoading(true);
            const data = await encuestasService.getPorEvento(eventoId);
            setEncuesta(data);

            if (data) {
                const resps = await encuestasService.getRespuestas(data.id);
                setRespuestas(resps || []);
            }
        } catch (error: any) {
            if (error.response?.status !== 404) {
                console.error('Error cargando encuesta:', error);
                toast.error('Error al cargar la encuesta');
            }
        } finally {
            setLoading(false);
        }
    };

    const handleAddPregunta = () => {
        setPreguntas([...preguntas, { enunciado: '', tipo: TipoPregunta.Estrellas }]);
    };

    const handleRemovePregunta = (index: number) => {
        setPreguntas(preguntas.filter((_, i) => i !== index));
    };

    const handleUpdatePregunta = (index: number, field: string, value: any) => {
        const newPreguntas = [...preguntas];
        (newPreguntas[index] as any)[field] = value;
        setPreguntas(newPreguntas);
    };

    const handleCrearEncuesta = async () => {
        if (!titulo.trim()) {
            toast.error('El título es obligatorio');
            return;
        }
        if (preguntas.length === 0) {
            toast.error('Debes añadir al menos una pregunta');
            return;
        }

        try {
            setSubmitting(true);
            await encuestasService.crear({
                eventoId,
                titulo,
                preguntas
            });
            toast.success('¡Encuesta creada con éxito!');
            await cargarEncuesta();
        } catch (error) {
            console.error('Error creando encuesta:', error);
            toast.error('No se pudo crear la encuesta');
        } finally {
            setSubmitting(false);
        }
    };

    const handlePublicar = async () => {
        if (!encuesta) return;
        try {
            setSubmitting(true);
            await encuestasService.publicar(encuesta.id);
            toast.success('¡Encuesta publicada!');
            await cargarEncuesta();
        } catch (error) {
            toast.error('No se pudo publicar la encuesta');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center p-12">
                <Loader2 className="w-8 h-8 text-blue-500 animate-spin mb-4" />
                <p className="text-slate-400 font-bold text-xs uppercase tracking-widest">Cargando...</p>
            </div>
        );
    }

    if (encuesta) {
        return (
            <div className="space-y-6">
                {/* Header Premium */}
                <div className="bg-[#1a1a20] border border-white/5 rounded-[2.5rem] p-8 shadow-2xl relative overflow-hidden group">
                    <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-500 to-indigo-600" />

                    <div className="flex flex-col md:flex-row items-center justify-between gap-8 relative z-10">
                        <div className="flex items-center gap-6">
                            <div className={`w-16 h-16 rounded-3xl flex items-center justify-center shadow-xl transition-transform group-hover:rotate-3 ${encuesta.publicada ? 'bg-emerald-500 shadow-emerald-500/20' : 'bg-amber-500 shadow-amber-500/20'}`}>
                                <Zap className="text-white w-8 h-8 fill-current" />
                            </div>
                            <div>
                                <h3 className="text-3xl font-black text-white tracking-tighter uppercase">{encuesta.titulo}</h3>
                                <div className="flex items-center gap-4 mt-2">
                                    <span className={`text-[10px] font-black uppercase tracking-[0.2em] px-3 py-1 rounded-full border ${encuesta.publicada ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-amber-500/10 border-amber-500/30 text-amber-400'}`}>
                                        {encuesta.publicada ? 'PUBLICADA Y ACTIVA' : 'BORRADOR'}
                                    </span>
                                    <div className="flex items-center gap-2 text-slate-500 text-[10px] font-black uppercase tracking-[0.2em]">
                                        <BarChart3 className="w-3 h-3" />
                                        {respuestas.length} RESPUESTAS RECIBIDAS
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="flex items-center gap-4">
                            {!encuesta.publicada && (
                                <button
                                    onClick={handlePublicar}
                                    disabled={submitting}
                                    className="bg-emerald-600 hover:bg-emerald-500 text-white px-8 py-4 rounded-2xl font-black uppercase tracking-widest text-xs shadow-xl shadow-emerald-600/20 transition-all flex items-center gap-2"
                                >
                                    {submitting ? <Loader2 className="animate-spin w-4 h-4" /> : <Save className="w-4 h-4" />}
                                    PUBLICAR AHORA
                                </button>
                            )}
                            <button
                                onClick={() => setShowResults(!showResults)}
                                className={`px-8 py-4 rounded-2xl font-black uppercase tracking-widest text-xs transition-all flex items-center gap-3 border ${showResults
                                        ? 'bg-blue-600 text-white border-blue-500 shadow-xl shadow-blue-600/20'
                                        : 'bg-slate-900 text-slate-400 border-white/5 hover:border-white/10'
                                    }`}
                            >
                                <BarChart3 className="w-4 h-4" />
                                {showResults ? 'Configuración' : 'Ver Resultados'}
                            </button>
                        </div>
                    </div>
                </div>

                {showResults ? (
                    /* Dashboard de Resultados Premium */
                    <div className="space-y-6 animate-in fade-in slide-in-from-right-4 duration-500 pb-10">
                        <h4 className="text-slate-500 font-black text-[10px] uppercase tracking-[0.3em] ml-2">Historial de Participaciones</h4>

                        {respuestas.length === 0 ? (
                            <div className="bg-[#121215] border border-white/5 border-dashed rounded-[2.5rem] p-20 text-center">
                                <p className="text-slate-600 font-bold uppercase tracking-widest">Aún no hay respuestas para mostrar</p>
                            </div>
                        ) : (
                            <div className="grid gap-4">
                                {respuestas.map((resp: any) => (
                                    <div key={resp.id} className="bg-[#121215] border border-white/5 rounded-3xl overflow-hidden transition-all hover:bg-[#16161a]">
                                        <button
                                            onClick={() => setExpandedResp(expandedResp === resp.id ? null : resp.id)}
                                            className="w-full p-6 flex flex-col md:flex-row md:items-center justify-between text-left gap-4"
                                        >
                                            <div className="flex items-center gap-4">
                                                <div className="w-12 h-12 bg-white/5 rounded-2xl flex items-center justify-center">
                                                    <User className="w-6 h-6 text-slate-500" />
                                                </div>
                                                <div>
                                                    <p className="text-white font-black text-xs uppercase tracking-widest">Participante {resp.usuarioId.substring(0, 5)}</p>
                                                    <div className="flex items-center gap-2 mt-1 py-1 px-3 bg-blue-500/10 rounded-lg w-fit">
                                                        <Clock className="w-3 h-3 text-blue-500" />
                                                        <span className="text-[9px] font-black text-blue-500 uppercase tracking-widest">
                                                            {new Date(resp.fecha).toLocaleDateString()} — {new Date(resp.fecha).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className="flex items-center gap-4">
                                                <div className="p-2 bg-slate-800/50 rounded-lg">
                                                    {expandedResp === resp.id ? <ChevronUp className="w-4 h-4 text-slate-400" /> : <ChevronDown className="w-4 h-4 text-slate-400" />}
                                                </div>
                                            </div>
                                        </button>

                                        {expandedResp === resp.id && (
                                            <div className="px-6 pb-8 pt-0 space-y-4 animate-in slide-in-from-top-4">
                                                <div className="h-px bg-white/5 w-full mb-6"></div>
                                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                                    {encuesta.preguntas.map((pregunta: any) => {
                                                        const valorAsociado = resp.valores.find((v: any) =>
                                                            String(v.preguntaId).toLowerCase() === String(pregunta.id).toLowerCase()
                                                        )?.valor;

                                                        const esEstrellas = String(pregunta.tipo).toLowerCase() === 'estrellas' || Number(pregunta.tipo) === 0;
                                                        const valorNumerico = Number(valorAsociado) || 0;

                                                        return (
                                                            <div key={pregunta.id} className="bg-black/20 p-6 rounded-2xl border border-white/5 shadow-inner">
                                                                <p className="text-slate-500 font-bold text-[10px] uppercase tracking-widest mb-4">{pregunta.enunciado}</p>
                                                                {esEstrellas ? (
                                                                    <div className="flex flex-col gap-3">
                                                                        <div className="flex gap-2">
                                                                            {[1, 2, 3, 4, 5].map(s => {
                                                                                const isFull = s <= valorNumerico;
                                                                                return (
                                                                                    <Star
                                                                                        key={s}
                                                                                        size={28}
                                                                                        stroke={isFull ? "#FFD700" : "#334155"}
                                                                                        fill={isFull ? "#FFD700" : "none"}
                                                                                        strokeWidth={2}
                                                                                        style={{
                                                                                            filter: isFull ? 'drop-shadow(0 0 5px rgba(255, 215, 0, 0.4))' : 'none',
                                                                                            transition: 'all 0.3s ease'
                                                                                        }}
                                                                                    />
                                                                                );
                                                                            })}
                                                                        </div>
                                                                        <div className="flex items-center gap-2 mt-1">
                                                                            <span className="text-[#FFD700] font-black text-2xl tracking-tighter">
                                                                                {valorNumerico}
                                                                            </span>
                                                                            <span className="text-slate-600 font-black text-sm uppercase tracking-widest">/ 5 puntos</span>
                                                                        </div>
                                                                    </div>
                                                                ) : (
                                                                    <div className="flex gap-3 bg-black/40 p-5 rounded-xl border border-white/5 group/msg">
                                                                        <MessageSquare className="w-5 h-5 text-blue-500 shrink-0 mt-0.5 group-hover/msg:scale-110 transition-transform" />
                                                                        <p className="text-slate-200 text-sm font-medium leading-relaxed italic">
                                                                            "{valorAsociado || '(Sin respuesta)'}"
                                                                        </p>
                                                                    </div>
                                                                )}
                                                            </div>
                                                        );
                                                    })}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                ) : (
                    /* Configuración View */
                    <div className="bg-[#121215] border border-white/5 rounded-[2.5rem] p-10 animate-in fade-in slide-in-from-left-4 duration-500">
                        <h4 className="text-slate-500 font-black text-[10px] uppercase tracking-[0.3em] mb-8">Estructura de la Encuesta</h4>
                        <div className="grid gap-4">
                            {encuesta.preguntas.map((p: any, idx) => (
                                <div key={p.id} className="bg-black/20 border border-white/5 p-6 rounded-2xl flex items-center gap-5 group/item">
                                    <div className="text-4xl font-black text-white/5 leading-none transition-colors group-hover/item:text-blue-500/10">0{idx + 1}</div>
                                    <div className="w-10 h-10 bg-slate-900 rounded-xl flex items-center justify-center border border-white/5 group-hover/item:border-blue-500/30 transition-all">
                                        {String(p.tipo).toLowerCase() === 'estrellas' || Number(p.tipo) === 0 ? <Star size={20} color="#f59e0b" fill="#f59e0b20" /> : <Type className="w-4 h-4 text-blue-400" />}
                                    </div>
                                    <span className="text-slate-200 font-bold uppercase tracking-tight text-sm">{p.enunciado}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        );
    }

    return (
        <div className="space-y-8 animate-in slide-in-from-bottom-8 duration-700">
            <div className="bg-amber-500/10 border border-amber-500/30 rounded-[2rem] p-8 flex items-center gap-6">
                <div className="w-12 h-12 bg-amber-500 rounded-2xl flex items-center justify-center shadow-lg shadow-amber-500/20 shrink-0">
                    <AlertCircle className="text-white w-6 h-6" />
                </div>
                <p className="text-amber-400 text-sm font-bold uppercase tracking-wider leading-relaxed">Este evento aún no tiene una encuesta configurada. Crea una ahora para recolectar insights valiosos de tus asistentes.</p>
            </div>

            <div className="bg-[#121215] border border-white/5 rounded-[3rem] p-10 space-y-10 shadow-2xl relative overflow-hidden">
                <div className="absolute top-0 right-0 w-64 h-64 bg-blue-600/5 blur-[100px] rounded-full"></div>

                <div className="relative z-10">
                    <label className="block text-slate-500 font-black text-[10px] uppercase tracking-[0.3em] mb-4">Título del Formulario</label>
                    <input
                        type="text"
                        value={titulo}
                        onChange={(e) => setTitulo(e.target.value)}
                        className="w-full bg-black/40 border border-white/5 rounded-2xl px-6 py-5 text-white outline-none focus:border-blue-500/50 transition-all font-black text-xl uppercase tracking-tighter"
                        placeholder="EJ: FEEDBACK DEL EVENTO"
                    />
                </div>

                <div className="space-y-6 relative z-10">
                    <div className="flex items-center justify-between px-2">
                        <label className="block text-slate-500 font-black text-[10px] uppercase tracking-[0.3em]">Preguntas Dinámicas</label>
                        <button
                            onClick={handleAddPregunta}
                            className="bg-blue-600/10 hover:bg-blue-600 text-blue-500 hover:text-white px-4 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all flex items-center gap-2"
                        >
                            <Plus className="w-3.5 h-3.5" /> AÑADIR PREGUNTA
                        </button>
                    </div>

                    <div className="grid gap-6">
                        {preguntas.map((p, i) => (
                            <div key={i} className="bg-black/20 border border-white/5 p-8 rounded-[2.5rem] group relative">
                                <div className="absolute top-8 left-0 w-1 h-12 bg-blue-600/20 group-hover:bg-blue-600 transition-all rounded-r-full"></div>

                                <div className="flex flex-col gap-6">
                                    <div className="flex items-center justify-between gap-4">
                                        <div className="flex items-center gap-4 flex-1">
                                            <span className="text-2xl font-black text-white/5">0{i + 1}</span>
                                            <input
                                                type="text"
                                                value={p.enunciado}
                                                onChange={(e) => handleUpdatePregunta(i, 'enunciado', e.target.value)}
                                                className="w-full bg-transparent border-b border-white/5 rounded-none px-0 py-2 text-lg text-white font-bold placeholder-slate-800 outline-none focus:border-blue-500/50 transition-all"
                                                placeholder="¿Qué deseas preguntar?"
                                            />
                                        </div>
                                        <button
                                            onClick={() => handleRemovePregunta(i)}
                                            className="p-3 bg-red-500/5 hover:bg-red-500 text-red-500 hover:text-white rounded-xl transition-all"
                                        >
                                            <Trash2 className="w-5 h-5" />
                                        </button>
                                    </div>

                                    <div className="flex gap-4">
                                        <button
                                            onClick={() => handleUpdatePregunta(i, 'tipo', TipoPregunta.Estrellas)}
                                            className={`flex-1 flex items-center justify-center gap-3 py-4 rounded-2xl text-[10px] font-black transition-all border ${p.tipo === TipoPregunta.Estrellas
                                                ? 'bg-amber-500 text-black border-amber-500 shadow-lg shadow-amber-500/20'
                                                : 'bg-black/40 border-white/5 text-slate-500 hover:border-white/20'}`}
                                        >
                                            <Star size={16} fill={p.tipo === TipoPregunta.Estrellas ? "currentColor" : "none"} /> VALORACIÓN (ESTRELLAS)
                                        </button>
                                        <button
                                            onClick={() => handleUpdatePregunta(i, 'tipo', TipoPregunta.Texto)}
                                            className={`flex-1 flex items-center justify-center gap-3 py-4 rounded-2xl text-[10px] font-black transition-all border ${p.tipo === TipoPregunta.Texto
                                                ? 'bg-blue-600 text-white border-blue-500 shadow-lg shadow-blue-600/20'
                                                : 'bg-black/40 border-white/5 text-slate-500 hover:border-white/20'}`}
                                        >
                                            <Type className="w-4 h-4" /> COMENTARIO LIBRE
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="pt-8 flex justify-end relative z-10">
                    <button
                        onClick={handleCrearEncuesta}
                        disabled={submitting}
                        className="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white px-12 py-5 rounded-3xl font-black uppercase tracking-[0.2em] text-sm shadow-2xl shadow-blue-600/30 transition-all flex items-center gap-4 disabled:opacity-50"
                    >
                        {submitting ? <Loader2 className="animate-spin w-6 h-6" /> : <Save className="w-6 h-6 shadow-[0_0_15px_rgba(255,255,255,0.3)]" />}
                        Configurar Encuesta
                    </button>
                </div>
            </div>
        </div>
    );
}
