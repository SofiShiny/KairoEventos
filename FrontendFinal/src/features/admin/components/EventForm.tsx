import { useState, useRef } from 'react';
import {
    X,
    Save,
    Upload,
    Calendar,
    MapPin,
    Type,
    FileText,
    Tag,
    Users,
    AlertCircle,
    Clock,
    DollarSign,
    ImageIcon,
    Video
} from 'lucide-react';
import { Evento } from '../../eventos/types/evento.types';
import { adminEventosService } from '../services/admin.eventos.service';

interface EventFormProps {
    evento?: Evento;
    onSuccess: () => void;
    onCancel: () => void;
}

export default function EventForm({ evento, onSuccess, onCancel }: EventFormProps) {
    const isEditing = !!evento;
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const fileInputRef = useRef<HTMLInputElement>(null);

    const [formData, setFormData] = useState({
        titulo: evento?.titulo || '',
        descripcion: evento?.descripcion || '',
        lugar: evento?.lugar || '',
        fechaInicio: evento?.fechaInicio ? new Date(evento.fechaInicio).toISOString().slice(0, 16) : '',
        fechaFin: evento?.fechaFin ? new Date(evento.fechaFin).toISOString().slice(0, 16) : '',
        maximoAsistentes: evento?.maximoAsistentes || 100,
        categoria: evento?.categoria || 'Conferencia',
        precioBase: 0, // Nuevo campo solicitado
        esVirtual: evento?.esVirtual || false,
    });

    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(evento?.imagenUrl || null);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleToggleVirtual = () => {
        setFormData(prev => ({ ...prev, esVirtual: !prev.esVirtual }));
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setImageFile(file);
            const reader = new FileReader();
            reader.onloadend = () => {
                setImagePreview(reader.result as string);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            if (!formData.titulo || !formData.fechaInicio || !formData.lugar) {
                throw new Error('Por favor completa todos los campos obligatorios.');
            }

            // Construir el DTO exacto que espera el Backend
            const eventDto = {
                titulo: formData.titulo,
                descripcion: formData.descripcion,
                ubicacion: {
                    nombreLugar: formData.lugar,
                    direccion: 'Dirección por defecto', // El backend lo marca como requerido
                    ciudad: 'Ciudad Ejemplo',
                    pais: 'VE'
                },
                fechaInicio: new Date(formData.fechaInicio).toISOString(),
                fechaFin: new Date(formData.fechaFin || formData.fechaInicio).toISOString(),
                maximoAsistentes: Number(formData.maximoAsistentes),
                categoria: formData.categoria,
                esVirtual: formData.esVirtual
            };

            let createdEvento: Evento;

            if (isEditing && evento.id) {
                createdEvento = await adminEventosService.updateEvento(evento.id, eventDto);
            } else {
                createdEvento = await adminEventosService.createEvento(eventDto);
            }

            // Paso 2: Subida de imagen si existe un archivo seleccionado
            if (imageFile && createdEvento.id) {
                try {
                    await adminEventosService.uploadImagen(createdEvento.id, imageFile);
                } catch (imgError) {
                    console.error('Evento creado pero error al subir imagen:', imgError);
                    // Opcional: Mostrar advertencia pero no fallar todo el proceso si el evento ya se creó
                }
            }

            onSuccess();
        } catch (err: any) {
            console.error(err);
            setError(err.response?.data || err.message || 'Error al procesar el evento');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4">
            <div
                className="absolute inset-0 bg-black/80 backdrop-blur-sm animate-in fade-in duration-300"
                onClick={onCancel}
            />

            <div className="relative w-full max-w-4xl bg-[#16191f] border border-slate-800 rounded-3xl shadow-2xl overflow-hidden animate-in zoom-in-95 slide-in-from-bottom-5 duration-400">
                <div className="flex items-center justify-between px-8 py-6 border-b border-slate-800 bg-[#1a1e26]">
                    <div>
                        <h2 className="text-2xl font-black text-white">
                            {isEditing ? 'Editar Evento' : 'Crear Nuevo Evento'}
                        </h2>
                        <p className="text-slate-500 text-xs font-bold uppercase tracking-widest mt-1 italic">
                            {isEditing ? `ID: ${evento.id.slice(0, 8)}...` : 'Configuración de nuevo registro'}
                        </p>
                    </div>
                    <button
                        onClick={onCancel}
                        className="p-2 hover:bg-slate-800 text-slate-400 hover:text-white rounded-xl transition-all"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="flex flex-col md:flex-row max-h-[75vh] overflow-hidden">
                    {/* Lateral Izquierda: Imagen */}
                    <div className="w-full md:w-80 bg-slate-900/50 p-8 border-r border-slate-800 flex flex-col items-center">
                        <label className="text-xs font-black text-slate-500 uppercase tracking-widest mb-4 self-start">
                            Carátula del Evento
                        </label>

                        <div
                            onClick={() => fileInputRef.current?.click()}
                            className="relative w-full aspect-[3/4] rounded-2xl border-2 border-dashed border-slate-700 hover:border-blue-500/50 hover:bg-blue-500/5 transition-all cursor-pointer overflow-hidden group"
                        >
                            {imagePreview ? (
                                <>
                                    <img src={imagePreview} className="w-full h-full object-cover" alt="Preview" />
                                    <div className="absolute inset-0 bg-black/60 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                        <Upload className="w-8 h-8 text-white" />
                                    </div>
                                </>
                            ) : (
                                <div className="absolute inset-0 flex flex-col items-center justify-center p-4 text-center">
                                    <ImageIcon className="w-12 h-12 text-slate-700 mb-2" />
                                    <p className="text-xs font-bold text-slate-500 uppercase tracking-tighter">Click para subir imagen</p>
                                </div>
                            )}
                        </div>
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleFileChange}
                            accept="image/*"
                            className="hidden"
                        />
                        <p className="mt-4 text-[10px] text-slate-600 text-center leading-relaxed font-medium">Recomendado: 1200x1600px. Formatos aceptados: JPG, PNG, WEBP.</p>
                    </div>

                    {/* Central: Datos */}
                    <div className="flex-1 p-8 overflow-y-auto custom-scrollbar space-y-6 bg-slate-900/20">
                        {error && (
                            <div className="bg-rose-500/10 border border-rose-500/20 p-4 rounded-2xl flex items-center gap-3 text-rose-500 animate-in shake duration-500">
                                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                                <p className="text-sm font-bold">{error}</p>
                            </div>
                        )}

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="md:col-span-2 space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <Type className="w-3 h-3 text-blue-500" /> Título del Evento *
                                </label>
                                <input
                                    type="text"
                                    name="titulo"
                                    required
                                    value={formData.titulo}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all"
                                />
                            </div>

                            <div className="md:col-span-2 space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <FileText className="w-3 h-3 text-blue-500" /> Descripción
                                </label>
                                <textarea
                                    name="descripcion"
                                    rows={3}
                                    value={formData.descripcion}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all resize-none"
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <Calendar className="w-3 h-3 text-blue-500" /> Inicio *
                                </label>
                                <input
                                    type="datetime-local"
                                    name="fechaInicio"
                                    required
                                    value={formData.fechaInicio}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all [color-scheme:dark]"
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <Clock className="w-3 h-3 text-blue-500" /> Fin
                                </label>
                                <input
                                    type="datetime-local"
                                    name="fechaFin"
                                    value={formData.fechaFin}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all [color-scheme:dark]"
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <MapPin className="w-3 h-3 text-blue-500" /> Lugar *
                                </label>
                                <input
                                    type="text"
                                    name="lugar"
                                    required
                                    value={formData.lugar}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all"
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <Tag className="w-3 h-3 text-blue-500" /> Categoría
                                </label>
                                <select
                                    name="categoria"
                                    value={formData.categoria}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all"
                                >
                                    <option value="Conferencia">Conferencia</option>
                                    <option value="Concierto">Concierto</option>
                                    <option value="Taller">Taller</option>
                                    <option value="Deporte">Deporte</option>
                                </select>
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <Users className="w-3 h-3 text-blue-500" /> Aforo Máximo
                                </label>
                                <input
                                    type="number"
                                    name="maximoAsistentes"
                                    value={formData.maximoAsistentes}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all"
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-xs font-black text-slate-500 uppercase tracking-widest ml-1">
                                    <DollarSign className="w-3 h-3 text-blue-500" /> Precio Base ($)
                                </label>
                                <input
                                    type="number"
                                    name="precioBase"
                                    value={formData.precioBase}
                                    onChange={handleChange}
                                    className="w-full bg-[#0f1115] border border-slate-700 rounded-2xl px-4 py-3 text-sm text-white focus:border-blue-500 transition-all"
                                />
                            </div>

                            {/* Evento Virtual Switch */}
                            <div className="md:col-span-2 p-6 bg-blue-500/5 border border-blue-500/20 rounded-2xl flex items-center justify-between group transition-all hover:bg-blue-500/10">
                                <div className="flex items-center gap-4">
                                    <div className="w-12 h-12 bg-blue-500/20 rounded-xl flex items-center justify-center text-blue-400 group-hover:scale-110 transition-transform">
                                        <Video className="w-6 h-6" />
                                    </div>
                                    <div>
                                        <p className="text-sm font-black text-white uppercase tracking-tight">Evento Virtual (Streaming)</p>
                                        <p className="text-[10px] text-slate-500 font-bold uppercase tracking-widest leading-none mt-1">
                                            Se generará un link de Google Meet automáticamente
                                        </p>
                                    </div>
                                </div>
                                <label className="relative inline-flex items-center cursor-pointer">
                                    <input
                                        type="checkbox"
                                        checked={formData.esVirtual}
                                        onChange={handleToggleVirtual}
                                        className="sr-only peer"
                                    />
                                    <div className="w-14 h-7 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[4px] after:left-[4px] after:bg-white after:rounded-full after:h-5 after:w-6 after:transition-all peer-checked:bg-blue-600 shadow-inner"></div>
                                </label>
                            </div>
                        </div>
                    </div>
                </form>

                <div className="flex items-center justify-end gap-3 px-8 py-6 border-t border-slate-800 bg-[#1a1e26]">
                    <button
                        type="button"
                        onClick={onCancel}
                        disabled={loading}
                        className="px-6 py-3 text-sm font-bold text-slate-400 hover:text-white transition-colors"
                    >
                        Descartar
                    </button>
                    <button
                        type="submit"
                        disabled={loading}
                        onClick={handleSubmit}
                        className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 disabled:bg-slate-700 text-white px-8 py-3 rounded-2xl font-black text-sm shadow-xl shadow-blue-600/20 active:scale-95 transition-all"
                    >
                        {loading ? (
                            <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                        ) : (
                            <Save className="w-5 h-5" />
                        )}
                        {isEditing ? 'Guardar Cambios' : 'Lanzar Evento'}
                    </button>
                </div>
            </div>

            <style>{`
                .custom-scrollbar::-webkit-scrollbar { width: 5px; }
                .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
                .custom-scrollbar::-webkit-scrollbar-thumb { background: #334155; border-radius: 10px; }
                .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3b82f6; }
            `}</style>
        </div>
    );
}
