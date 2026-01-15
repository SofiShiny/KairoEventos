import React, { useState, useEffect } from 'react';
import { UserAvatar } from '../components/UserAvatar';
import { usuariosService, ActualizarPerfilDto, UsuarioDto } from '../services/usuarios.service';
import { useAuth } from '../contexts/AuthContext';
import { toast } from 'react-toastify';

export const ProfilePage: React.FC = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [perfil, setPerfil] = useState<UsuarioDto | null>(null);

    const [formData, setFormData] = useState<ActualizarPerfilDto>({
        nombre: '',
        telefono: '',
        direccion: '',
    });

    useEffect(() => {
        if (user?.id) {
            cargarPerfil();
        }
    }, [user]);

    const cargarPerfil = async () => {
        if (!user?.id) return;

        try {
            setLoading(true);
            const data = await usuariosService.getUsuario(user.id);
            setPerfil(data);
            setFormData({
                nombre: data.nombre || '',
                telefono: data.telefono?.valor || '',
                direccion: data.direccion?.valor || '',
            });
        } catch (error) {
            console.error('Error al cargar perfil:', error);
            toast.error('Error al cargar el perfil');
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!user?.id) return;

        try {
            setSaving(true);
            await usuariosService.actualizarPerfil(user.id, formData);
            toast.success('Perfil actualizado correctamente');
            await cargarPerfil(); // Recargar datos
        } catch (error) {
            console.error('Error al actualizar perfil:', error);
            toast.error('Error al actualizar el perfil');
        } finally {
            setSaving(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div>
            </div>
        );
    }

    const nombreCompleto = perfil?.nombre || user?.username || 'Usuario';

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-3xl mx-auto">
                {/* Header con Avatar */}
                <div className="bg-white rounded-2xl shadow-xl overflow-hidden mb-8">
                    <div className="bg-gradient-to-r from-purple-600 to-pink-600 h-32"></div>
                    <div className="px-8 pb-8">
                        <div className="flex flex-col sm:flex-row items-center sm:items-end -mt-16 sm:-mt-12">
                            <UserAvatar
                                name={nombreCompleto}
                                size="xl"
                                className="border-4 border-white"
                            />
                            <div className="mt-4 sm:mt-0 sm:ml-6 text-center sm:text-left">
                                <h1 className="text-3xl font-bold text-gray-900">{nombreCompleto}</h1>
                                <p className="text-gray-600 mt-1">@{perfil?.username || user?.username}</p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Formulario de Edición */}
                <div className="bg-white rounded-2xl shadow-xl p-8">
                    <h2 className="text-2xl font-bold text-gray-900 mb-6">
                        Editar Perfil
                    </h2>

                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* Nombre */}
                        <div>
                            <label htmlFor="nombre" className="block text-sm font-medium text-gray-700 mb-2">
                                Nombre Completo *
                            </label>
                            <input
                                type="text"
                                id="nombre"
                                name="nombre"
                                value={formData.nombre}
                                onChange={handleChange}
                                required
                                maxLength={100}
                                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                                placeholder="Tu nombre completo"
                            />
                        </div>

                        {/* Teléfono */}
                        <div>
                            <label htmlFor="telefono" className="block text-sm font-medium text-gray-700 mb-2">
                                Teléfono *
                            </label>
                            <input
                                type="tel"
                                id="telefono"
                                name="telefono"
                                value={formData.telefono}
                                onChange={handleChange}
                                required
                                maxLength={20}
                                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                                placeholder="+56 9 1234 5678"
                            />
                            <p className="text-sm text-gray-500 mt-1">
                                Solo dígitos (7-15 caracteres)
                            </p>
                        </div>

                        {/* Dirección */}
                        <div>
                            <label htmlFor="direccion" className="block text-sm font-medium text-gray-700 mb-2">
                                Dirección *
                            </label>
                            <input
                                type="text"
                                id="direccion"
                                name="direccion"
                                value={formData.direccion}
                                onChange={handleChange}
                                required
                                minLength={5}
                                maxLength={200}
                                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                                placeholder="Calle 123, Comuna, Ciudad"
                            />
                            <p className="text-sm text-gray-500 mt-1">
                                Mínimo 5 caracteres
                            </p>
                        </div>

                        {/* Botones */}
                        <div className="flex justify-end space-x-4 pt-6 border-t">
                            <button
                                type="button"
                                onClick={cargarPerfil}
                                disabled={saving}
                                className="px-6 py-3 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors disabled:opacity-50"
                            >
                                Cancelar
                            </button>
                            <button
                                type="submit"
                                disabled={saving}
                                className="px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-lg font-medium hover:from-purple-700 hover:to-pink-700 transition-all disabled:opacity-50 flex items-center space-x-2"
                            >
                                {saving ? (
                                    <>
                                        <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                        <span>Guardando...</span>
                                    </>
                                ) : (
                                    <span>Guardar Cambios</span>
                                )}
                            </button>
                        </div>
                    </form>
                </div>

                {/* Información Adicional */}
                <div className="mt-8 bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <div className="flex">
                        <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-blue-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm text-blue-700">
                                Tu información personal es privada y solo tú puedes verla y editarla.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
