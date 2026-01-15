import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import {
    Star,
    MessageSquare,
    Send,
    CheckCircle2,
    ChevronLeft,
    Sparkles,
    Loader2,
    Award
} from 'lucide-react';
import { encuestasService, Encuesta, TipoPregunta } from '../services/encuestas.service';
import { toast } from 'react-hot-toast';

export const EncuestaPage = () => {
    const { id: eventoId } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const auth = useAuth();

    const [encuesta, setEncuesta] = useState<Encuesta | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    const [respuestas, setRespuestas] = useState<Record<string, string>>({});

    const usuarioId = auth.user?.profile.sub;

    useEffect(() => {
        if (eventoId) {
            loadEncuesta();
        }
    }, [eventoId]);

    const esPreguntaEstrellas = (tipo: any) => {
        // Log para depuración
        console.log('DEBUG: Evaluando tipo de pregunta:', tipo);
        if (tipo === undefined || tipo === null) return false;

        const t = String(tipo).toLowerCase();
        return t === 'estrellas' || t === '0' || tipo === 0;
    };

    const loadEncuesta = async () => {
        try {
            setLoading(true);
            const data = await encuestasService.getPorEvento(eventoId!);

            console.log('DEBUG: Datos de la encuesta recibidos:', data);

            if (!data.publicada) {
                toast.error('Esta encuesta aún no ha sido activada por el organizador.');
                setTimeout(() => navigate('/perfil'), 2000);
                return;
            }

            setEncuesta(data);

            const initialRespuestas: Record<string, string> = {};
            data.preguntas?.forEach(p => {
                initialRespuestas[p.id] = esPreguntaEstrellas(p.tipo) ? '5' : '';
            });
            setRespuestas(initialRespuestas);
        } catch (error) {
            console.error('Error cargando encuesta:', error);
            toast.error('No se encontró una encuesta disponible para este evento.');
            setTimeout(() => navigate('/perfil'), 2000);
        } finally {
            setLoading(false);
        }
    };

    const handleAnswerChange = (preguntaId: string, valor: string) => {
        setRespuestas(prev => ({ ...prev, [preguntaId]: valor }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!auth.isAuthenticated) {
            auth.signinRedirect();
            return;
        }

        try {
            setSubmitting(true);
            const command = {
                encuestaId: encuesta!.id,
                usuarioId: usuarioId!,
                respuestas: Object.entries(respuestas).map(([preguntaId, valor]) => ({
                    preguntaId,
                    valor
                }))
            };

            await encuestasService.responder(command);
            setSubmitted(true);
            toast.success('¡Gracias por tus comentarios!');
        } catch (error: any) {
            console.error('Error enviando respuestas:', error);
            const mensaje = error.response?.data?.message || 'Error al enviar la encuesta.';
            toast.error(mensaje);
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-[#0a0a0c] flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-slate-500 font-bold uppercase tracking-widest text-xs">Cargando Encuesta...</p>
            </div>
        );
    }

    if (submitted) {
        return (
            <div className="min-h-screen bg-[#0a0a0c] flex items-center justify-center p-8">
                <div className="max-w-md w-full text-center bg-[#121215] border border-white/5 p-12 rounded-[3.5rem] shadow-2xl relative overflow-hidden">
                    <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-500 to-indigo-600" />
                    <div className="w-24 h-24 bg-green-500/10 rounded-full flex items-center justify-center mx-auto mb-8 border border-green-500/20">
                        <CheckCircle2 className="w-12 h-12 text-green-500" />
                    </div>
                    <h2 className="text-4xl font-black text-white mb-4 tracking-tighter uppercase">¡Gracias!</h2>
                    <p className="text-slate-400 font-medium mb-10 leading-relaxed">
                        Tu opinión es fundamental para seguir mejorando nuestras experiencias.
                    </p>
                    <button
                        onClick={() => navigate('/perfil')}
                        className="w-full py-4 bg-blue-600 text-white font-black rounded-2xl hover:bg-blue-500 transition-all uppercase tracking-widest text-xs shadow-lg shadow-blue-600/20"
                    >
                        Volver a mi perfil
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#0a0a0c] text-white p-8 selection:bg-blue-500/30">
            <div className="max-w-3xl mx-auto">
                <button
                    onClick={() => navigate(-1)}
                    className="flex items-center gap-2 text-slate-500 hover:text-white transition-colors mb-12 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-xs">Volver</span>
                </button>

                <header className="mb-16 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-blue-500" />
                        <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">Feedback Kairo</span>
                    </div>
                    <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                        {encuesta?.titulo || 'ENCUESTA DE SATISFACCIÓN'}
                    </h1>
                    <p className="text-slate-400 text-lg font-medium max-w-xl">
                        ¿Cómo fue tu experiencia? Ayúdanos a mejorar dedicando unos segundos a estas preguntas.
                    </p>
                </header>

                <form onSubmit={handleSubmit} className="space-y-12">
                    {encuesta?.preguntas.map((pregunta, index) => {
                        const esEstrellas = esPreguntaEstrellas(pregunta.tipo);
                        const valorActual = Number(respuestas[pregunta.id]) || 0;

                        return (
                            <div key={pregunta.id} className="bg-[#121215] border border-white/5 p-8 rounded-[2.5rem] transition-all duration-300 hover:border-white/10 shadow-xl group">
                                <div className="flex items-start gap-4 mb-10">
                                    <span className="text-4xl font-black text-white/5 leading-none transition-colors group-hover:text-blue-500/10">0{index + 1}</span>
                                    <h3 className="text-xl font-black text-white leading-tight uppercase tracking-tight">{pregunta.enunciado}</h3>
                                </div>

                                {esEstrellas ? (
                                    <div className="flex justify-center gap-4 flex-wrap">
                                        {[1, 2, 3, 4, 5].map(star => {
                                            const isActive = valorActual >= star;
                                            return (
                                                <button
                                                    key={star}
                                                    type="button"
                                                    onClick={() => handleAnswerChange(pregunta.id, star.toString())}
                                                    className={`w-14 h-14 rounded-2xl flex items-center justify-center transition-all duration-300 ${isActive
                                                            ? 'bg-amber-500 text-black shadow-[0_0_25px_rgba(245,158,11,0.4)] scale-110'
                                                            : 'bg-slate-900 text-slate-600 hover:bg-slate-800 border border-white/5'
                                                        }`}
                                                >
                                                    <Star className={`w-8 h-8 ${isActive ? 'fill-current' : ''}`} />
                                                </button>
                                            );
                                        })}
                                    </div>
                                ) : (
                                    <div className="relative">
                                        <div className="absolute top-4 left-4 p-2 bg-blue-500/10 rounded-lg">
                                            <MessageSquare className="w-4 h-4 text-blue-500" />
                                        </div>
                                        <textarea
                                            value={respuestas[pregunta.id] || ''}
                                            onChange={(e) => handleAnswerChange(pregunta.id, e.target.value)}
                                            placeholder="Comparte tu opinión con nosotros..."
                                            className="w-full bg-[#0a0a0c] border border-white/5 rounded-2xl p-4 pl-12 text-white placeholder-slate-700 focus:border-blue-500 min-h-[140px] transition-all outline-none"
                                        />
                                    </div>
                                )}
                            </div>
                        );
                    })}

                    <div className="pt-8">
                        <button
                            type="submit"
                            disabled={submitting}
                            className="w-full py-6 bg-gradient-to-r from-blue-600 to-indigo-600 text-white font-black rounded-3xl hover:from-blue-500 hover:to-indigo-500 transition-all uppercase tracking-[0.2em] shadow-xl flex items-center justify-center gap-3 disabled:opacity-50"
                        >
                            {submitting ? (
                                <Loader2 className="w-6 h-6 animate-spin" />
                            ) : (
                                <>
                                    ENVIAR FEEDBACK <Send className="w-5 h-5" />
                                </>
                            )}
                        </button>

                        <div className="mt-8 flex items-center justify-center gap-3 text-slate-600">
                            <Award className="w-4 h-4 text-blue-500/50" />
                            <p className="text-[10px] font-black uppercase tracking-[0.2em]">Tu opinión nos ayuda a crear eventos legendarios</p>
                        </div>
                    </div>
                </form>
            </div>
        </div>
    );
};
