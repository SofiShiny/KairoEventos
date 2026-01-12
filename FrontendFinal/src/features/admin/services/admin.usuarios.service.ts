import api from '@/lib/axios';

export interface CreateUserRequest {
    username: string;
    email: string;
    nombre: string;
    telefono: string;
    direccion: string;
    password: string;
    role: 'organizador' | 'admin';
}

export interface Usuario {
    id: string;
    username: string;
    email: string;
    nombre: string;
    enabled: boolean;
    roles: string[];
    createdTimestamp: number;
}

class AdminUsuariosService {
    /**
     * Obtener todos los usuarios
     */
    async getUsuarios(): Promise<Usuario[]> {
        try {
            const response = await api.get('/usuarios');
            return response.data.data || response.data;
        } catch (error: any) {
            console.error('Error al obtener usuarios:', error);
            throw new Error(error.response?.data?.message || 'Error al cargar usuarios');
        }
    }

    /**
     * Crear un nuevo usuario (organizador o admin)
     */
    async crearUsuario(userData: CreateUserRequest): Promise<Usuario> {
        try {
            // Transformar datos del frontend al formato del backend
            const backendPayload = {
                username: userData.username,
                nombre: userData.nombre,
                correo: userData.email,
                telefono: userData.telefono,
                direccion: userData.direccion,
                rol: this.mapRoleToEnum(userData.role),
                password: userData.password
            };

            const response = await api.post('/usuarios', backendPayload);
            return response.data.data || response.data;
        } catch (error: any) {
            console.error('Error al crear usuario:', error);
            throw new Error(error.response?.data?.message || 'Error al crear usuario');
        }
    }

    /**
     * Mapea el rol del frontend al enum del backend
     */
    private mapRoleToEnum(role: string): number {
        // Enum Rol en el backend: User = 1, Admin = 2, Organizator = 3
        const roleMap: Record<string, number> = {
            'admin': 2,
            'organizador': 3
        };
        return roleMap[role.toLowerCase()] || 1; // Default: User
    }

    /**
     * Actualizar un usuario existente
     */
    async actualizarUsuario(userId: string, userData: Partial<CreateUserRequest>): Promise<Usuario> {
        try {
            const response = await api.put(`/usuarios/${userId}`, userData);
            return response.data.data || response.data;
        } catch (error: any) {
            console.error('Error al actualizar usuario:', error);
            throw new Error(error.response?.data?.message || 'Error al actualizar usuario');
        }
    }

    /**
     * Eliminar un usuario
     */
    async eliminarUsuario(userId: string): Promise<void> {
        try {
            await api.delete(`/usuarios/${userId}`);
        } catch (error: any) {
            console.error('Error al eliminar usuario:', error);
            throw new Error(error.response?.data?.message || 'Error al eliminar usuario');
        }
    }


    /**
     * Asignar rol a un usuario
     */
    async asignarRol(userId: string, role: string): Promise<void> {
        try {
            await api.post(`/usuarios/${userId}/roles`, { role });
        } catch (error: any) {
            console.error('Error al asignar rol:', error);
            throw new Error(error.response?.data?.message || 'Error al asignar rol');
        }
    }
}

export const adminUsuariosService = new AdminUsuariosService();
