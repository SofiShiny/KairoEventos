import { useState, useEffect } from 'react';
import { Users, UserPlus, Shield, Mail, Trash2 } from 'lucide-react';
import { adminUsuariosService, Usuario, CreateUserRequest } from '../services/admin.usuarios.service';

import { useT } from '../../../i18n';

export default function AdminUsuarios() {
    const t = useT();
    const [usuarios, setUsuarios] = useState<Usuario[]>([]);
    const [loading, setLoading] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [formData, setFormData] = useState<CreateUserRequest>({
        username: '',
        email: '',
        nombre: '',
        telefono: '',
        direccion: '',
        password: '',
        role: 'organizador'
    });
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        loadUsuarios();
    }, []);

    const loadUsuarios = async () => {
        try {
            setLoading(true);
            const data = await adminUsuariosService.getUsuarios();
            setUsuarios(data);
        } catch (error: any) {
            console.error('Error al cargar usuarios:', error);
            alert(`${t.adminUsers.modal.error}: ` + error.message);
        } finally {
            setLoading(false);
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.username.trim()) {
            newErrors.username = t.adminUsers.validation.usernameRequired;
        } else if (formData.username.length < 3) {
            newErrors.username = t.adminUsers.validation.usernameMin;
        }

        if (!formData.email.trim()) {
            newErrors.email = t.adminUsers.validation.emailRequired;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            newErrors.email = t.adminUsers.validation.emailInvalid;
        }

        if (!formData.nombre.trim()) {
            newErrors.nombre = t.adminUsers.validation.nameRequired;
        }

        if (!formData.telefono.trim()) {
            newErrors.telefono = t.adminUsers.validation.phoneRequired;
        } else if (!/^\d{10}$/.test(formData.telefono.replace(/\D/g, ''))) {
            newErrors.telefono = t.adminUsers.validation.phoneInvalid;
        }

        if (!formData.direccion.trim()) {
            newErrors.direccion = t.adminUsers.validation.addressRequired;
        }

        if (!formData.password) {
            newErrors.password = t.adminUsers.validation.passwordRequired;
        } else if (formData.password.length < 8) {
            newErrors.password = t.adminUsers.validation.passwordMin;
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) return;

        setSubmitting(true);
        try {
            await adminUsuariosService.crearUsuario(formData);
            alert(`✅ ${t.adminUsers.modal.success}`);
            setShowCreateModal(false);
            setFormData({
                username: '',
                email: '',
                nombre: '',
                telefono: '',
                direccion: '',
                password: '',
                role: 'organizador'
            });
            loadUsuarios();
        } catch (error: any) {
            alert(`❌ ${t.adminUsers.modal.error}: ` + error.message);
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (userId: string, username: string) => {
        if (!confirm(t.adminUsers.delete.confirm.replace('{username}', username))) return;

        try {
            await adminUsuariosService.eliminarUsuario(userId);
            alert(`✅ ${t.adminUsers.delete.success}`);
            loadUsuarios();
        } catch (error: any) {
            alert(`❌ ${t.adminUsers.delete.error}: ` + error.message);
        }
    };

    const getRoleBadgeColor = (roles: string[] | undefined) => {
        if (!roles || !Array.isArray(roles)) return 'bg-gray-500/20 text-gray-400 border-gray-500/50';
        if (roles.includes('admin')) return 'bg-red-500/20 text-red-400 border-red-500/50';
        if (roles.includes('organizador')) return 'bg-purple-500/20 text-purple-400 border-purple-500/50';
        return 'bg-gray-500/20 text-gray-400 border-gray-500/50';
    };

    const getRoleLabel = (roles: string[] | undefined) => {
        if (!roles || !Array.isArray(roles)) return t.adminUsers.roles.user;
        if (roles.includes('admin')) return t.adminUsers.roles.admin;
        if (roles.includes('organizador')) return t.adminUsers.roles.organizer;
        return t.adminUsers.roles.user;
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <div className="text-center">
                    <div className="inline-block animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-purple-500 mb-4"></div>
                    <p className="text-gray-400">{t.common.loading}...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white p-8">
            {/* Header */}
            <div className="mb-8">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-4xl font-black bg-gradient-to-r from-purple-400 to-pink-500 bg-clip-text text-transparent flex items-center gap-3">
                            <Users className="w-10 h-10 text-purple-500" />
                            {t.adminUsers.title}
                        </h1>
                        <p className="text-gray-400 mt-2">{t.adminUsers.description}</p>
                    </div>
                    <button
                        onClick={() => setShowCreateModal(true)}
                        className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 rounded-xl font-semibold transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-purple-500/50"
                    >
                        <UserPlus className="w-5 h-5" />
                        {t.adminUsers.createNew}
                    </button>
                </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-xl p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">{t.adminUsers.totalUsers}</p>
                            <p className="text-3xl font-bold text-white mt-1">{usuarios.length}</p>
                        </div>
                        <Users className="w-12 h-12 text-purple-500 opacity-50" />
                    </div>
                </div>
                <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-xl p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">{t.adminUsers.organizers}</p>
                            <p className="text-3xl font-bold text-white mt-1">
                                {usuarios.filter(u => u.roles?.includes('organizador')).length}
                            </p>
                        </div>
                        <Shield className="w-12 h-12 text-blue-500 opacity-50" />
                    </div>
                </div>
                <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-xl p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">{t.adminUsers.admins}</p>
                            <p className="text-3xl font-bold text-white mt-1">
                                {usuarios.filter(u => u.roles?.includes('admin')).length}
                            </p>
                        </div>
                        <Shield className="w-12 h-12 text-red-500 opacity-50" />
                    </div>
                </div>
            </div>

            {/* Tabla de Usuarios */}
            <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-2xl overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full">
                        <thead className="bg-black/50 border-b border-gray-800">
                            <tr>
                                <th className="px-6 py-4 text-left text-sm font-semibold text-gray-400">{t.adminUsers.table.user}</th>
                                <th className="px-6 py-4 text-left text-sm font-semibold text-gray-400">{t.adminUsers.table.email}</th>
                                <th className="px-6 py-4 text-left text-sm font-semibold text-gray-400">{t.adminUsers.table.role}</th>
                                <th className="px-6 py-4 text-left text-sm font-semibold text-gray-400">{t.adminUsers.table.created}</th>
                                <th className="px-6 py-4 text-right text-sm font-semibold text-gray-400">{t.adminUsers.table.actions}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-800">
                            {usuarios.map((usuario) => (
                                <tr key={usuario.id} className="hover:bg-gray-900/50 transition">
                                    <td className="px-6 py-4">
                                        <div>
                                            <p className="font-semibold text-white">{usuario.username}</p>
                                            <p className="text-sm text-gray-400">{usuario.nombre}</p>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-2 text-gray-300">
                                            <Mail className="w-4 h-4 text-gray-500" />
                                            {usuario.email}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${getRoleBadgeColor(usuario.roles)}`}>
                                            {getRoleLabel(usuario.roles)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-gray-400">
                                        {new Date(usuario.createdTimestamp).toLocaleDateString(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US')}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center justify-end gap-2">
                                            <button
                                                onClick={() => handleDelete(usuario.id, usuario.username)}
                                                className="p-2 hover:bg-red-900/20 rounded-lg transition"
                                                title={t.common.delete}
                                            >
                                                <Trash2 className="w-5 h-5 text-red-400" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Modal Crear Usuario */}
            {showCreateModal && (
                <div className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-2xl max-w-md w-full shadow-2xl">
                        <div className="p-6 border-b border-gray-800">
                            <h2 className="text-2xl font-bold text-white flex items-center gap-2">
                                <UserPlus className="w-6 h-6 text-purple-500" />
                                {t.adminUsers.modal.title}
                            </h2>
                            <p className="text-gray-400 text-sm mt-1">{t.adminUsers.modal.description}</p>
                        </div>

                        <form onSubmit={handleSubmit} className="p-6 space-y-4">
                            {/* Username */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.username} *
                                </label>
                                <input
                                    type="text"
                                    value={formData.username}
                                    onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.username ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder="usuario123"
                                />
                                {errors.username && <p className="mt-1 text-sm text-red-400">{errors.username}</p>}
                            </div>

                            {/* Email */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.table.email} *
                                </label>
                                <input
                                    type="email"
                                    value={formData.email}
                                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.email ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder="usuario@ejemplo.com"
                                />
                                {errors.email && <p className="mt-1 text-sm text-red-400">{errors.email}</p>}
                            </div>

                            {/* Nombre Completo */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.fullName} *
                                </label>
                                <input
                                    type="text"
                                    value={formData.nombre}
                                    onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.nombre ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder="Juan Pérez"
                                />
                                {errors.nombre && <p className="mt-1 text-sm text-red-400">{errors.nombre}</p>}
                            </div>

                            {/* Teléfono */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.phone} *
                                </label>
                                <input
                                    type="tel"
                                    value={formData.telefono}
                                    onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.telefono ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder="1234567890"
                                />
                                {errors.telefono && <p className="mt-1 text-sm text-red-400">{errors.telefono}</p>}
                            </div>

                            {/* Dirección */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.address} *
                                </label>
                                <input
                                    type="text"
                                    value={formData.direccion}
                                    onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.direccion ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder="Calle 123, Ciudad"
                                />
                                {errors.direccion && <p className="mt-1 text-sm text-red-400">{errors.direccion}</p>}
                            </div>

                            {/* Password */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.password} *
                                </label>
                                <input
                                    type="password"
                                    value={formData.password}
                                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                    className={`w-full px-4 py-3 bg-black/50 border ${errors.password ? 'border-red-500' : 'border-gray-700'} rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500`}
                                    placeholder={t.adminUsers.validation.passwordMin}
                                />
                                {errors.password && <p className="mt-1 text-sm text-red-400">{errors.password}</p>}
                            </div>

                            {/* Rol */}
                            <div>
                                <label className="block text-sm font-medium text-gray-300 mb-2">
                                    {t.adminUsers.modal.role} *
                                </label>
                                <select
                                    value={formData.role}
                                    onChange={(e) => setFormData({ ...formData, role: e.target.value as 'organizador' | 'admin' })}
                                    className="w-full px-4 py-3 bg-black/50 border border-gray-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-purple-500"
                                >
                                    <option value="organizador">{t.adminUsers.roles.organizer}</option>
                                    <option value="admin">{t.adminUsers.roles.admin}</option>
                                </select>
                            </div>

                            {/* Botones */}
                            <div className="flex gap-3 pt-4">
                                <button
                                    type="button"
                                    onClick={() => setShowCreateModal(false)}
                                    className="flex-1 px-4 py-3 bg-gray-800 hover:bg-gray-700 text-white rounded-lg transition"
                                    disabled={submitting}
                                >
                                    {t.common.cancel}
                                </button>
                                <button
                                    type="submit"
                                    className="flex-1 px-4 py-3 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white font-semibold rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
                                    disabled={submitting}
                                >
                                    {submitting ? t.adminUsers.modal.creating : t.adminUsers.createNew}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
