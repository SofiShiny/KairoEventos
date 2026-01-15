import { useState, useEffect } from 'react';
import {
    Plus,
    Trash2,
    Save,
    CheckCircle,
    AlertCircle,
    Loader2,
    Star,
    Type
} from 'lucide-react';
import { encuestasService, Encuesta, TipoPregunta } from '../../encuestas/services/encuestas.service';
import { toast } from 'react-hot-toast';

interface AdminEncuestasManagerProps {
    eventoId: string;
}

export default function AdminEncuestasManager({ eventoId }: AdminEncuestasManagerProps) {
    const [encuesta, setEncuesta] = useState<Encuesta | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

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
        if (preguntas.some(p => !p.enunciado.trim())) {
            toast.error('Todas las preguntas deben tener un enunciado');
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
            toast.success('¡Encuesta publicada y ahora es visible para los asistentes!');
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
                <p className="text-slate-400 font-bold text-xs uppercase tracking-widest">Cargando Encuestas...</p>
            </div>
        );
    }

    if (encuesta) {
        return (
            <div className="space-y-6 animate-in fade-in duration-500">
                <div className="bg-emerald-500/5 border border-emerald-500/20 rounded-2xl p-6 flex flex-col md:flex-row items-center justify-between gap-4">
                    <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-emerald-500 rounded-2xl flex items-center justify-center shadow-lg shadow-emerald-500/20">
                            <CheckCircle className="text-white w-6 h-6" />
                        </div>
                        <div>
                            <h3 className="text-xl font-bold text-emerald-400">{encuesta.titulo}</h3>
                            <p className="text-slate-500 text-sm font-medium">Esta encuesta ya ha sido creada.</p>
                        </div>
                    </div>

                    {!encuesta.publicada && (
                        <button
                            onClick={handlePublicar}
                            disabled={submitting}
                            className="bg-emerald-600 hover:bg-emerald-500 text-white px-6 py-3 rounded-xl font-black uppercase tracking-widest text-xs shadow-lg shadow-emerald-600/20 transition-all flex items-center gap-2"
                        >
                            {submitting ? <Loader2 className="animate-spin w-4 h-4" /> : <Save className="w-4 h-4" />}
                            Publicar Ahora
                        </button>
                    )}
                    {encuesta.publicada && (
                        <div className="px-5 py-2.5 bg-emerald-500/20 text-emerald-500 rounded-full border border-emerald-500/30 text-[10px] font-black uppercase tracking-[0.2em]">
                            Encuesta Publicada
                        </div>
                    )}
                </div>

                <div className="grid gap-4">
                    <h4 className="text-slate-500 font-black text-xs uppercase tracking-widest mb-2">Preguntas Configuradas</h4>
                    {encuesta.preguntas.map((p: any) => (
                        <div key={p.id} className="bg-slate-900 border border-slate-800 p-5 rounded-2xl flex items-center gap-4">
                            <div className="w-8 h-8 bg-slate-800 rounded-lg flex items-center justify-center">
                                {Number(p.tipo) === TipoPregunta.Estrellas ? <Star className="w-4 h-4 text-amber-500" /> : <Type className="w-4 h-4 text-blue-400" />}
                            </div>
                            <span className="text-slate-200 font-medium">{p.enunciado}</span>
                        </div>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-8 animate-in slide-in-from-bottom-4 duration-500">
            <div className="bg-amber-500/5 border border-amber-500/20 rounded-2xl p-6 flex items-center gap-4">
                <AlertCircle className="text-amber-500 w-6 h-6" />
                <p className="text-amber-500/80 text-sm font-medium">Este evento aún no tiene una encuesta configurada. Crea una ahora para recibir feedback de los asistentes.</p>
            </div>

            <div className="space-y-6">
                <div>
                    <label className="block text-slate-500 font-black text-[10px] uppercase tracking-[0.2em] mb-2">Título de la Encuesta</label>
                    <input
                        type="text"
                        value={titulo}
                        onChange={(e) => setTitulo(e.target.value)}
                        className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-white outline-none focus:border-blue-500/50 transition-all font-bold"
                        placeholder="Ej: Feedback del Evento"
                    />
                </div>

                <div className="space-y-4">
                    <div className="flex items-center justify-between">
                        <label className="block text-slate-500 font-black text-[10px] uppercase tracking-[0.2em]">Preguntas</label>
                        <button
                            onClick={handleAddPregunta}
                            className="text-blue-400 hover:text-blue-300 text-[10px] font-black uppercase tracking-widest flex items-center gap-1.5"
                        >
                            <Plus className="w-3.5 h-3.5" /> Añadir Pregunta
                        </button>
                    </div>

                    <div className="space-y-4">
                        {preguntas.map((p, i) => (
                            <div key={i} className="bg-slate-900 border border-slate-800 p-6 rounded-2xl group relative overflow-hidden">
                                <div className="absolute top-0 left-0 w-1 h-full bg-blue-600/20 group-hover:bg-blue-600 transition-all"></div>

                                <div className="flex flex-col md:flex-row gap-4">
                                    <div className="flex-1 space-y-4">
                                        <input
                                            type="text"
                                            value={p.enunciado}
                                            onChange={(e) => handleUpdatePregunta(i, 'enunciado', e.target.value)}
                                            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-2 text-sm text-white placeholder-slate-700 outline-none focus:border-blue-500/40"
                                            placeholder={`Pregunta #${i + 1}`}
                                        />

                                        <div className="flex gap-4">
                                            <button
                                                onClick={() => handleUpdatePregunta(i, 'tipo', TipoPregunta.Estrellas)}
                                                className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-xl text-[10px] font-black transition-all border ${p.tipo === TipoPregunta.Estrellas
                                                    ? 'bg-amber-500/10 border-amber-500/50 text-amber-500'
                                                    : 'bg-slate-950 border-slate-800 text-slate-500 hover:border-slate-700'}`}
                                            >
                                                <Star className="w-3 h-3" /> VALORACIÓN (ESTRELLAS)
                                            </button>
                                            <button
                                                onClick={() => handleUpdatePregunta(i, 'tipo', TipoPregunta.Texto)}
                                                className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-xl text-[10px] font-black transition-all border ${p.tipo === TipoPregunta.Texto
                                                    ? 'bg-blue-500/10 border-blue-500/50 text-blue-400'
                                                    : 'bg-slate-950 border-slate-800 text-slate-500 hover:border-slate-700'}`}
                                            >
                                                <Type className="w-3 h-3" /> TEXTO ABIERTO
                                            </button>
                                        </div>
                                    </div>

                                    <button
                                        onClick={() => handleRemovePregunta(i)}
                                        className="p-3 bg-rose-500/10 hover:bg-rose-500/20 text-rose-500 rounded-xl transition-all self-start"
                                    >
                                        <Trash2 className="w-5 h-5" />
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="pt-6 border-t border-slate-800 flex justify-end">
                    <button
                        onClick={handleCrearEncuesta}
                        disabled={submitting}
                        className="bg-blue-600 hover:bg-blue-500 text-white px-8 py-4 rounded-2xl font-black uppercase tracking-widest text-sm shadow-xl shadow-blue-600/20 transition-all flex items-center gap-3 disabled:opacity-50"
                    >
                        {submitting ? <Loader2 className="animate-spin w-5 h-5" /> : <Save className="w-5 h-5" />}
                        Configurar Encuesta
                    </button>
                </div>
            </div>
        </div>
    );
}
