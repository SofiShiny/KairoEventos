import { useState, useEffect } from 'react';
import { useAuth } from 'react-oidc-context';
import { useNavigate } from 'react-router-dom';
import {
    User,
    Mail,
    Phone,
    MapPin,
    Save,
    ChevronLeft,
    Sparkles,
    Loader2,
    Lock,
    Eye,
    EyeOff,
    CheckCircle2
} from 'lucide-react';
import { usuariosService, ActualizarPerfilDto, CambiarPasswordDto } from '../services/usuarios.service';
import { toast } from 'react-hot-toast';

export const ProfileEditPage = () => {
    const auth = useAuth();
    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [tab, setTab] = useState<'info' | 'password'>('info');

    // Profile data
    const [formData, setFormData] = useState({
        nombre: '',
        telefono: '',
        direccion: ''
    });

    // Password data
    const [passwordData, setPasswordData] = useState({
        passwordActual: '',
        nuevoPassword: '',
        confirmarPassword: ''
    });

    const [showPasswords, setShowPasswords] = useState({
        actual: false,
        nuevo: false,
        confirmar: false
    });

    const usuarioId = auth.user?.profile.sub;
    const email = auth.user?.profile.email;
    const username = (auth.user?.profile as any)?.preferred_username || 'Usuario';

    useEffect(() => {
        if (usuarioId) {
            loadUserData();
        }
    }, [usuarioId]);

    const loadUserData = async () => {
        try {
            setLoading(true);
            const usuario = await usuariosService.getUsuario(usuarioId!);
            setFormData({
                nombre: usuario.nombre || '',
                telefono: usuario.telefono || '',
                direccion: usuario.direccion || ''
            });
        } catch (error) {
            console.error('Error cargando datos del usuario:', error);
            toast.error('Error al cargar tu perfil');
        } finally {
            setLoading(false);
        }
    };

    const handleSaveProfile = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!formData.nombre.trim()) {
            toast.error('El nombre es obligatorio');
            return;
        }

        try {
            setSaving(true);
            const dto: ActualizarPerfilDto = {
                nombre: formData.nombre,
                telefono: formData.telefono,
                direccion: formData.direccion
            };

            await usuariosService.actualizarPerfil(usuarioId!, dto);
            toast.success('¡Perfil actualizado con éxito!');
        } catch (error) {
            toast.error('Error al actualizar el perfil');
        } finally {
            setSaving(false);
        }
    };

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!passwordData.passwordActual || !passwordData.nuevoPassword) {
            toast.error('Completa todos los campos');
            return;
        }

        if (passwordData.nuevoPassword.length < 8) {
            toast.error('La nueva contraseña debe tener al menos 8 caracteres');
            return;
        }

        if (passwordData.nuevoPassword !== passwordData.confirmarPassword) {
            toast.error('Las contraseñas no coinciden');
            return;
        }

        try {
            setSaving(true);
            const dto: CambiarPasswordDto = {
                passwordActual: passwordData.passwordActual,
                nuevoPassword: passwordData.nuevoPassword
            };

            await usuariosService.cambiarPassword(usuarioId!, dto);
            toast.success('¡Contraseña actualizada con éxito!');
            setPasswordData({
                passwordActual: '',
                nuevoPassword: '',
                confirmarPassword: ''
            });
        } catch (error: any) {
            if (error.response?.status === 400) {
                toast.error('Contraseña actual incorrecta');
            } else {
                toast.error('Error al cambiar la contraseña');
            }
        } finally {
            setSaving(false);
        }
    };

    if (!auth.isAuthenticated) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <div className="text-center">
                    <User className="w-16 h-16 text-neutral-700 mx-auto mb-4" />
                    <p className="text-neutral-500 font-bold">Debes iniciar sesión</p>
                </div>
            </div>
        );
    }

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">Cargando perfil...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white p-8">
            <div className="max-w-4xl mx-auto">
                <button
                    onClick={() => navigate('/perfil')}
                    className="flex items-center gap-2 text-neutral-500 hover:text-white transition-colors mb-12 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-xs">Volver al perfil</span>
                </button>

                <header className="mb-16 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-blue-500" />
                        <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">Configuración</span>
                    </div>
                    <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                        EDITAR PERFIL
                    </h1>
                    <p className="text-neutral-500 text-lg font-medium max-w-xl">
                        Actualiza tu información personal y preferencias de seguridad.
                    </p>
                </header>

                {/* Tabs */}
                <div className="flex gap-4 mb-10 overflow-x-auto pb-2">
                    <button
                        onClick={() => setTab('info')}
                        className={`px-8 py-3 rounded-2xl font-black text-sm uppercase tracking-widest transition-all ${tab === 'info' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        Información Personal
                    </button>
                    <button
                        onClick={() => setTab('password')}
                        className={`px-8 py-3 rounded-2xl font-black text-sm uppercase tracking-widest transition-all ${tab === 'password' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        Cambiar Contraseña
                    </button>
                </div>

                {/* Profile Info Tab */}
                {tab === 'info' && (
                    <form onSubmit={handleSaveProfile} className="space-y-8">
                        <div className="bg-neutral-900/50 border border-neutral-800 rounded-[2.5rem] p-8">
                            {/* Username (readonly) */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Usuario
                                </label>
                                <div className="relative">
                                    <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type="text"
                                        value={username}
                                        disabled
                                        className="w-full bg-neutral-800/50 border border-neutral-700 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 cursor-not-allowed opacity-60"
                                    />
                                </div>
                            </div>

                            {/* Email (readonly) */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Correo Electrónico
                                </label>
                                <div className="relative">
                                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type="email"
                                        value={email || ''}
                                        disabled
                                        className="w-full bg-neutral-800/50 border border-neutral-700 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 cursor-not-allowed opacity-60"
                                    />
                                </div>
                            </div>

                            {/* Nombre */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Nombre Completo *
                                </label>
                                <div className="relative">
                                    <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type="text"
                                        value={formData.nombre}
                                        onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                        placeholder="Ingresa tu nombre completo"
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                                        required
                                    />
                                </div>
                            </div>

                            {/* Teléfono */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Teléfono
                                </label>
                                <div className="relative">
                                    <Phone className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type="tel"
                                        value={formData.telefono}
                                        onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                                        placeholder="+1 (555) 123-4567"
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                                    />
                                </div>
                            </div>

                            {/* Dirección */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Dirección
                                </label>
                                <div className="relative">
                                    <MapPin className="absolute left-4 top-4 w-5 h-5 text-neutral-600" />
                                    <textarea
                                        value={formData.direccion}
                                        onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
                                        placeholder="Calle, número, ciudad, código postal..."
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all min-h-[100px] resize-none"
                                    />
                                </div>
                            </div>

                            {/* Save Button */}
                            <button
                                type="submit"
                                disabled={saving}
                                className="w-full py-4 bg-gradient-to-r from-blue-600 to-blue-800 text-white font-black rounded-2xl hover:from-blue-500 hover:to-blue-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-3 uppercase tracking-widest text-xs shadow-xl"
                            >
                                {saving ? (
                                    <Loader2 className="w-5 h-5 animate-spin" />
                                ) : (
                                    <>
                                        <Save className="w-5 h-5" />
                                        GUARDAR CAMBIOS
                                    </>
                                )}
                            </button>
                        </div>
                    </form>
                )}

                {/* Password Tab */}
                {tab === 'password' && (
                    <form onSubmit={handleChangePassword} className="space-y-8">
                        <div className="bg-neutral-900/50 border border-neutral-800 rounded-[2.5rem] p-8">
                            {/* Current Password */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Contraseña Actual *
                                </label>
                                <div className="relative">
                                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type={showPasswords.actual ? 'text' : 'password'}
                                        value={passwordData.passwordActual}
                                        onChange={(e) => setPasswordData({ ...passwordData, passwordActual: e.target.value })}
                                        placeholder="Ingresa tu contraseña actual"
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-12 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                                        required
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowPasswords({ ...showPasswords, actual: !showPasswords.actual })}
                                        className="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-600 hover:text-white transition-colors"
                                    >
                                        {showPasswords.actual ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                                    </button>
                                </div>
                            </div>

                            {/* New Password */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Nueva Contraseña *
                                </label>
                                <div className="relative">
                                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type={showPasswords.nuevo ? 'text' : 'password'}
                                        value={passwordData.nuevoPassword}
                                        onChange={(e) => setPasswordData({ ...passwordData, nuevoPassword: e.target.value })}
                                        placeholder="Mínimo 8 caracteres"
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-12 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                                        required
                                        minLength={8}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowPasswords({ ...showPasswords, nuevo: !showPasswords.nuevo })}
                                        className="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-600 hover:text-white transition-colors"
                                    >
                                        {showPasswords.nuevo ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                                    </button>
                                </div>
                                <p className="text-xs text-neutral-500 mt-2 ml-1">Debe tener al menos 8 caracteres</p>
                            </div>

                            {/* Confirm Password */}
                            <div className="mb-8">
                                <label className="block text-xs font-black uppercase tracking-widest text-neutral-400 mb-3">
                                    Confirmar Nueva Contraseña *
                                </label>
                                <div className="relative">
                                    <CheckCircle2 className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                                    <input
                                        type={showPasswords.confirmar ? 'text' : 'password'}
                                        value={passwordData.confirmarPassword}
                                        onChange={(e) => setPasswordData({ ...passwordData, confirmarPassword: e.target.value })}
                                        placeholder="Repite la nueva contraseña"
                                        className="w-full bg-neutral-800 border border-neutral-700 rounded-2xl pl-12 pr-12 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                                        required
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowPasswords({ ...showPasswords, confirmar: !showPasswords.confirmar })}
                                        className="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-600 hover:text-white transition-colors"
                                    >
                                        {showPasswords.confirmar ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                                    </button>
                                </div>
                            </div>

                            {/* Security Info */}
                            <div className="mb-8 p-4 bg-blue-500/10 border border-blue-500/20 rounded-2xl">
                                <div className="flex gap-3">
                                    <Lock className="w-5 h-5 text-blue-400 flex-shrink-0 mt-0.5" />
                                    <div className="text-xs text-neutral-400">
                                        <p className="font-bold text-blue-400 mb-1">Seguridad de tu cuenta</p>
                                        <p>Tu contraseña se almacena de forma segura y encriptada. Nunca la compartiremos con nadie.</p>
                                    </div>
                                </div>
                            </div>

                            {/* Save Button */}
                            <button
                                type="submit"
                                disabled={saving}
                                className="w-full py-4 bg-gradient-to-r from-purple-600 to-blue-600 text-white font-black rounded-2xl hover:from-purple-500 hover:to-blue-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-3 uppercase tracking-widest text-xs shadow-xl"
                            >
                                {saving ? (
                                    <Loader2 className="w-5 h-5 animate-spin" />
                                ) : (
                                    <>
                                        <Lock className="w-5 h-5" />
                                        CAMBIAR CONTRASEÑA
                                    </>
                                )}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};
